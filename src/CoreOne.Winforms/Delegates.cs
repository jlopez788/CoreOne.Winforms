namespace CoreOne.Winforms;

public delegate void AsyncRun(Action action);

public delegate T AsyncRun<T>(Func<T> action);