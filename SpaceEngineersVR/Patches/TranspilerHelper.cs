using HarmonyLib;
using SpaceEngineersVR.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SpaceEngineersVR.Patches.TranspilerHelper
{
	//An index to a code instruction. May become invalidated if the code is mutated!
	public struct CodeInstructionIndex
	{
		public CodeInstructionIndex(int index)
		{
			this.index = index;
		}

		public int index;

		public CodeGap before => new CodeGap(index);
		public CodeGap after => new CodeGap(index + 1);

		public CodeInstructionIndex previous => new CodeInstructionIndex(index - 1);
		public CodeInstructionIndex next => new CodeInstructionIndex(index + 1);
	}

	//A position pointing between two code instructions, for insertions and ranges. May become invalidated if the code is mutated!
	public struct CodeGap
	{
		public CodeGap(int index)
		{
			this.index = index;
		}

		public int index;

		public CodeInstructionIndex before => new CodeInstructionIndex(index - 1);
		public CodeInstructionIndex after => new CodeInstructionIndex(index);

		public CodeGap previous => new CodeGap(index - 1);
		public CodeGap next => new CodeGap(index + 1);
	}

	//A range of code instructions. May become invalid if the code is mutated!
	public struct CodeRange : IEnumerable<CodeInstructionIndex>
	{
		public CodeRange(CodeGap begin, int count)
		{
			this.begin = begin;
			this.count = count;
		}
		public CodeRange(CodeGap begin, CodeGap end)
		{
			this.begin = begin;
			count = end.index - begin.index;
		}

		public CodeRange(CodeInstructionIndex first, int count)
		{
			begin = first.before;
			this.count = count;
		}
		public CodeRange(CodeInstructionIndex first, CodeInstructionIndex last)
		{
			begin = first.before;
			count = last.before.index - begin.index;
		}

		public CodeGap begin { get; set; }
		public CodeGap end
		{
			get => new CodeGap(begin.index + count);
			set => count = value.index - begin.index;
		}
		public int count { get; set; }

		public CodeInstructionIndex first
		{
			get => begin.after;
			set => begin = value.before;
		}
		public CodeInstructionIndex last
		{
			get => end.before;
			set => end = value.after;
		}

		public bool Contains(CodeInstructionIndex code) => code.index >= first.index && code.index <= last.index;

		public IEnumerable<CodeInstructionIndex> Each()
		{
			for (int i = begin.after.index; i <= end.before.index; ++i)
			{
				yield return new CodeInstructionIndex(i);
			}
		}
		public IEnumerable<CodeInstructionIndex> Reverse()
		{
			for (int i = end.before.index; i >= begin.after.index; --i)
			{
				yield return new CodeInstructionIndex(i);
			}
		}

		public IEnumerator<CodeInstructionIndex> GetEnumerator()
		{
			return Each().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Each().GetEnumerator();
		}
	}

	public enum MoveLabels
	{
		LeaveAfterInsertedCode, //Explicitly do not move labels
		MoveToInsertedCode, //Labels will be moved from the instruction after the specified insertion position to the first newly inserted code instruction
	}
	public enum MoveBlocks
	{
		LeaveAfterInsertedCode, //Explicitly do not move blocks
		MoveToInsertedCode, //Blocks will be moved from the instruction after the specified insertion position to the first newly inserted code instruction
	}

	public class TranspilerHelper : IEnumerable<CodeInstruction>
	{
		public TranspilerHelper(IEnumerable<CodeInstruction> instructions)
		{
			this.instructions = new List<CodeInstruction>(instructions);
		}

		private readonly List<CodeInstruction> instructions;


		public CodeRange All() => new CodeRange(Begin(), instructions.Count);

		public CodeInstructionIndex First() => new CodeInstructionIndex(0);
		public CodeInstructionIndex Last() => new CodeInstructionIndex(instructions.Count - 1);

		public CodeGap Begin() => new CodeGap(0);
		public CodeGap End() => new CodeGap(instructions.Count);

		//Inserts the code into the given position, moving labels and blocks if requested
		//Returns a range covering the inserted code
		public CodeRange Insert(CodeGap position, MoveLabels moveLabels, MoveBlocks moveBlocks, params CodeInstruction[] code)
		{
			if (moveLabels == MoveLabels.MoveToInsertedCode)
				code[0].MoveLabelsFrom(this[position.after]);
			if (moveBlocks == MoveBlocks.MoveToInsertedCode)
				code[0].MoveBlocksFrom(this[position.after]);

			instructions.InsertRange(position.index, code);
			return new CodeRange(position, code.Length);
		}
		//Inserts the code into the given position, moving labels and blocks if requested, and shifts the given position to point after the newly inserted code
		//Returns a range covering the inserted code
		public CodeRange InsertAndAdvance(ref CodeGap position, MoveLabels moveLabels, MoveBlocks moveBlocks, params CodeInstruction[] code)
		{
			CodeRange range = Insert(position, moveLabels, moveBlocks, code);
			position.index += code.Length;
			return range;
		}

		//Removes a single code instruction, moving labels and blocks to the code instruction index given
		public void Remove(CodeInstructionIndex remove, CodeInstructionIndex moveLabelsAndBlocksTo)
		{
			if (moveLabelsAndBlocksTo.index == remove.index)
				throw new ArgumentException("Can not move labels to a code instruction which will be removed", "moveLabelsAndBlocksTo");

			this[moveLabelsAndBlocksTo].labels.AddRange(this[remove].labels);
			this[moveLabelsAndBlocksTo].blocks.AddRange(this[remove].blocks);
			instructions.RemoveAt(remove.index);
		}
		//Removes a range of code instructions, moving labels and blocks to the code instruction index given
		public void Remove(CodeRange range, CodeInstructionIndex moveLabelsAndBlocksTo)
		{
			if (range.Contains(moveLabelsAndBlocksTo))
				throw new ArgumentException("Can not move labels to a code instruction which will be removed", "moveLabelsAndBlocksTo");

			for (int i = 0; i < range.count; ++i)
			{
				this[moveLabelsAndBlocksTo].labels.AddRange(instructions[range.begin.index + i].labels);
				this[moveLabelsAndBlocksTo].blocks.AddRange(instructions[range.begin.index + i].blocks);
			}
			instructions.RemoveRange(range.begin.index, range.count);
		}

		//Replaces a single code instruction with another, moving labels and blocks to the new instruction
		public void Replace(CodeInstructionIndex replace, CodeInstruction replaceWith)
		{
			if (replaceWith.labels.IsNullOrEmpty())
				replaceWith.labels = this[replace].labels;
			else
				replaceWith.labels.AddRange(this[replace].labels);

			if (replaceWith.blocks.IsNullOrEmpty())
				replaceWith.blocks = this[replace].blocks;
			else
				replaceWith.blocks.AddRange(this[replace].blocks);

			instructions[replace.index] = replaceWith;
		}

		//Replaces a range of code instructions from the given first instruction, with a count equal to the number of instructions given. I.e. the final instruction count remains unchanged
		//Labels and blocks are moved to the new instructions in the same positions as they were before
		public CodeRange ReplaceRange(CodeInstructionIndex first, params CodeInstruction[] code)
		{
			for (int i = 0; i < code.Length; ++i)
			{
				Replace(new CodeInstructionIndex(first.index + i), code[i]);
			}
			return new CodeRange(first, code.Length);
		}
		//Replaces a range of code instructions from the given begining of the range, with a count equal to the number of instructions given. I.e. the final instruction count remains unchanged
		//Labels and blocks are moved to the new instructions in the same positions as they were before
		public CodeRange ReplaceRange(CodeGap begin, params CodeInstruction[] code)
		{
			for (int i = 0; i < code.Length; ++i)
			{
				Replace(new CodeInstructionIndex(begin.after.index + i), code[i]);
			}
			return new CodeRange(First(), code.Length);
		}

		/*
		TODO: move labels and blocks somewhere?? not sure how to decide where. parameter? first code statement being replaced?
		public CodeRange ReplaceRange(CodePosition start, int count, params CodeInstruction[] code)
		{
			int diff = count - code.Length;
			if (diff < 0)
			{
				instructions.RemoveRange(start.index, -diff);
			}
			else if (diff > 0)
			{
				instructions.InsertRange(start.index + count, code.Skip(diff));
			}
			for (int i = 0; i < count; ++i)
			{
				instructions[start.index + i] = code[i];
			}
		}
		//start is inclusive, end is exclusive
		public CodeRange ReplaceRange(CodePosition start, CodePosition end, params CodeInstruction[] code)
		{
			return ReplaceRange(start, end.index - start.index, code);
		}
		*/


		//Checks if each instruction from (and including) the given first instruction matches the corrisponding predicate
		public bool InstructionsPassPredicates(CodeInstructionIndex first, params Predicate<CodeInstruction>[] predicates)
		{
			for (int i = 0; i < predicates.Length; ++i)
			{
				if (!predicates[i](this[new CodeInstructionIndex(first.index + i)]))
					return false;
			}
			return true;
		}

		//Finds the first code instruction index within the given range that matches the predicate, if any
		public CodeInstructionIndex? FindFirst(CodeRange withinRange, Predicate<CodeInstruction> predicate)
		{
			foreach (CodeInstructionIndex i in withinRange)
			{
				if (predicate(this[i]))
					return i;
			}
			return null;
		}
		//Finds the first code instruction index after the given position that matches the predicate, if any
		public CodeInstructionIndex? FindFirst(CodeGap after, Predicate<CodeInstruction> predicate)
		{
			return FindFirst(new CodeRange(after, End()), predicate);
		}
		//Finds the first code instruction index that matches the predicate, if any
		public CodeInstructionIndex? FindFirst(Predicate<CodeInstruction> predicate)
		{
			return FindFirst(All(), predicate);
		}

		//Finds the first range of code instructions within the given range that match the predicates in order, if any
		public CodeRange? FindFirst(CodeRange withinRange, params Predicate<CodeInstruction>[] predicates)
		{
			foreach (CodeInstructionIndex i in withinRange)
			{
				if (InstructionsPassPredicates(i, predicates))
					return new CodeRange(i, predicates.Length);
			}
			return null;
		}
		//Finds the first range of code instructions after the given position that match the predicates in order, if any
		public CodeRange? FindFirst(CodeGap after, params Predicate<CodeInstruction>[] predicates)
		{
			return FindFirst(new CodeRange(after, End()), predicates);
		}
		//Finds the first range of code instructions that match the predicates in order, if any
		public CodeRange? FindFirst(params Predicate<CodeInstruction>[] predicates)
		{
			return FindFirst(All(), predicates);
		}

		//Finds the last code instruction index within the given range that matches the predicate, if any
		public CodeInstructionIndex? FindLast(CodeRange withinRange, Predicate<CodeInstruction> predicate)
		{
			foreach (CodeInstructionIndex i in withinRange.Reverse())
			{
				if (predicate(this[i]))
					return i;
			}
			return null;
		}
		//Finds the last code instruction index before the given position that matches the predicate, if any
		public CodeInstructionIndex? FindLast(CodeGap before, Predicate<CodeInstruction> predicate)
		{
			return FindLast(new CodeRange(Begin(), before), predicate);
		}
		//Finds the last code instruction index that matches the predicate, if any
		public CodeInstructionIndex? FindLast(Predicate<CodeInstruction> predicate)
		{
			return FindLast(All(), predicate);
		}

		//Finds the last range of code instructions within the given range that match the predicates in order, if any
		public CodeRange? FindLast(CodeRange range, params Predicate<CodeInstruction>[] predicates)
		{
			foreach (CodeInstructionIndex i in range.Reverse())
			{
				if (InstructionsPassPredicates(i, predicates))
					return new CodeRange(i, predicates.Length);
			}
			return null;
		}
		//Finds the last range of code instructions before the given position that match the predicates in order, if any
		public CodeRange? FindLast(CodeGap before, params Predicate<CodeInstruction>[] predicates)
		{
			return FindLast(new CodeRange(Begin(), before), predicates);
		}
		//Finds the last range of code instructions that match the predicates in order, if any
		public CodeRange? FindLast(params Predicate<CodeInstruction>[] predicates)
		{
			return FindLast(All(), predicates);
		}

		//Yields each code instruction within the given range that matches the given predicate
		public IEnumerable<CodeInstructionIndex> FindEach(CodeRange withinRange, Predicate<CodeInstruction> predicate)
		{
			foreach (CodeInstructionIndex i in withinRange)
			{
				if (predicate(this[i]))
					yield return i;
			}
		}
		//Yields each code instruction that matches the given predicate
		public IEnumerable<CodeInstructionIndex> FindEach(Predicate<CodeInstruction> predicate)
		{
			return FindEach(All(), predicate);
		}
		//Yields each range of instructions within the given range that match the given predicates in order
		public IEnumerable<CodeRange> FindEach(CodeRange withinRange, params Predicate<CodeInstruction>[] predicates)
		{
			foreach (CodeInstructionIndex i in withinRange)
			{
				if (InstructionsPassPredicates(i, predicates))
					yield return new CodeRange(i, predicates.Length);
			}
		}
		//Yields each range of instructions that match the given predicates in order
		public IEnumerable<CodeRange> FindEach(params Predicate<CodeInstruction>[] predicates)
		{
			return FindEach(All(), predicates);
		}

		//Yields each code instruction within the given range that matches the given predicate, in reversed order. Useful if you will mutate the contents
		public IEnumerable<CodeInstructionIndex> FindEachReversed(CodeRange withinRange, Predicate<CodeInstruction> predicate)
		{
			foreach (CodeInstructionIndex i in withinRange.Reverse())
			{
				if (predicate(this[i]))
					yield return i;
			}
		}
		//Yields each code instruction that matches the given predicate, in reversed order. Useful if you will mutate the contents
		public IEnumerable<CodeInstructionIndex> FindEachReversed(Predicate<CodeInstruction> predicate)
		{
			return FindEachReversed(All(), predicate);
		}
		//Yields each range of instructions within the given range that match the given predicate, in reversed order. Useful if you will mutate the contents
		public IEnumerable<CodeRange> FindEachReversed(CodeRange withinRange, params Predicate<CodeInstruction>[] predicates)
		{
			foreach (CodeInstructionIndex i in withinRange.Reverse())
			{
				if (InstructionsPassPredicates(i, predicates))
					yield return new CodeRange(i, predicates.Length);
			}
		}
		//Yields each range of instructions that match the given predicate, in reversed order. Useful if you will mutate the contents
		public IEnumerable<CodeRange> FindEachReversed(params Predicate<CodeInstruction>[] predicates)
		{
			return FindEachReversed(All(), predicates);
		}


		//Returns the index and label that a branch goes to, or null if it is not a branch
		public (CodeInstructionIndex index, Label label)? FollowBranch(CodeInstructionIndex branchIndex)
		{
			if (this[branchIndex].Branches(out Label? label))
			{
				foreach (CodeInstructionIndex i in new CodeRange(branchIndex, Last()))
				{
					if (this[i].labels.Contains(label.Value))
						return (i, label.Value);
				}
			}

			return null;
		}

		//Returns the index and label after an if (and possibly else) statement, or null if it is not a branch
		public (CodeInstructionIndex index, Label label)? GetPositionAfterIfAndElse(CodeInstructionIndex ifPos)
		{
			(CodeInstructionIndex index, Label label)? ifEndPos = FollowBranch(ifPos);
			if (ifEndPos != null)
				return FollowBranch(ifEndPos.Value.index.previous) ?? ifEndPos;

			return null;
		}

		/*
		I thought this was working, but after testing, it doesnt seem to be
		public (CodeInstructionIndex index, Label label)? ReplaceIfElseByJumping(CodeRange ifRange)
		{
			//if-else statements typically look like this:
			//
			//prep stack for check
			//jump to "second" if true/false
			//contents when true
			//jump to "end"
			//"second":
			//contents if false
			//"end":
			//
			//so, we need to:
			//remove stack prep
			//find the destination of the conditional jump
			//check one instruction behind that to see if it is a second jump
			//		if so, find the destination of that
			//replace conditional jump with unconditional jump to the last jump destination
			//
			//final code should look like this:
			//
			//jump to "end"
			//contents when true
			//jump to "end"
			//"second":
			//contents if false
			//"end":
			//
			//leaving the code inside the if/else blocks untouched helps reduce conflicts with other transpilers


			Remove(new CodeRange(ifRange.begin, ifRange.count - 1), ifRange.last);
			CodeInstructionIndex jumpIndex = ifRange.first;

			(CodeInstructionIndex index, Label label)? jumpTo = FollowBranch(jumpIndex);
			if (jumpTo != null)
			{
				this[jumpIndex].opcode = OpCodes.Br_S;

				CodeInstructionIndex secondJump = jumpTo.Value.index.previous;
				(CodeInstructionIndex pos, Label label)? elseEnd = FollowBranch(secondJump);
				if (elseEnd != null && this[secondJump].BranchesUnconditionally())
				{
					this[jumpIndex].operand = elseEnd.Value.label;
					return elseEnd;
				}

				return jumpTo;
			}
			return null;
		}
		*/


		public CodeInstruction this[CodeInstructionIndex pos] => instructions[pos.index];


		public IEnumerator<CodeInstruction> GetEnumerator()
		{
			return ((IEnumerable<CodeInstruction>)instructions).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)instructions).GetEnumerator();
		}
	}

	public static class CodeInstructionExtensions
	{
		public static bool BranchesUnconditionally(this CodeInstruction self)
		{
			return self.opcode == OpCodes.Br || self.opcode == OpCodes.Br_S;
		}
		public static bool BranchesUnconditionally(this CodeInstruction self, out Label? label)
		{
			if (BranchesUnconditionally(self))
			{
				label = self.operand as Label?;
				return true;
			}
			label = null;
			return false;
		}
	}
}
