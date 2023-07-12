using System;

namespace SpaceEngineersVR.Util;

public enum LeftRight
{
	Left,
	Right,
}

public static class LeftRightExtensions
{
	public static LeftRight Invert(this LeftRight lr)
	{
		return lr switch
		{
			LeftRight.Left => LeftRight.Right,
			_ => LeftRight.Left,
		};
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
		get => lr switch
		{
			LeftRight.Left => left,
			_ => right,
		};
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
		return new(func(left), func(right));
	}
}
