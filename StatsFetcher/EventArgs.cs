﻿using System;

namespace StatsFetcher
{
  public class EventArgs<T> : EventArgs
  {
    public T Data { get; private set; }

    public EventArgs(T input)
    {
      Data = input;
    }
  }
}