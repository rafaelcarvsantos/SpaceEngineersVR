using System;

namespace SpaceEngineersVR.Util
{
	public enum LeftRight
	{
		Left,
		Right,
	}

	public static class LeftRightExtensions
	{
		public static LeftRight Invert(this LeftRight lr)
		{
			switch (lr)
			{
				case LeftRight.Left: return LeftRight.Right;
				default:
				case LeftRight.Right: return LeftRight.Left;
			}
		}
	}

	public struct Handed<T>
	{
		public Handed(T left, T right)
		{
			this.left = left;
			this.right = right;
		}

		public T left;
		public T right;

		public T Primary => this[Player.Player.Handedness];
		public T Secondary => this[Player.Player.Handedness.Invert()];

		public T this[LeftRight lr]
		{
			get
			{
				switch (lr)
				{
					case LeftRight.Left: return left;
					default:
					case LeftRight.Right: return right;
				}
			}

			set
			{
				switch (lr)
				{
					case LeftRight.Left: left = value; break;
					default:
					case LeftRight.Right: right = value; break;
				}
			}
		}

		public void Invoke(Action<T> func)
		{
			func(left);
			func(right);
		}
		public Handed<TResult> Invoke<TResult>(Func<T, TResult> func)
		{
			return new Handed<TResult>(func(left), func(right));
		}
	}
}
