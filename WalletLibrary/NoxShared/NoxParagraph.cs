using NoxKeys;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WalletLibrary.NoxShared
{

  internal class NoxStackArray : IDisposable
  {
    StackObject[] stackObjects;
    public NoxStackArray(NoxManagedArray noxManagedArray)
    {
      stackObjects = new StackObject[noxManagedArray.Value.Length];
      int stkcount = 0;

      for (int i = noxManagedArray.Value.Length - 1; i > -1; i--)
      {
        StackObject stackObject = new StackObject();
        if (noxManagedArray.Value[i] == 32) stackObject.IsSpaceChar = true;
        stackObject.BT = noxManagedArray.Value[i];
        stackObjects[stkcount] = stackObject;
        stkcount++;
      }
    }
    public StackObject[] Value
    {
      get
      {
        return stackObjects;
      }
    }
    public void Dispose()
    {
      try
      {
        int WriteInt = 0;
        for (int i = 0; i < stackObjects.Length; i++)
        {
          stackObjects[i].BT = (byte)WriteInt;
          stackObjects[i].IsSpaceChar = false;
          if (WriteInt == 0)
          {
            WriteInt = 1;
          }
          else
          {
            WriteInt = 0;
          }
        }
      }
      catch { }

    }
  }
  public class StackObject
  {
    public byte BT { get; set; }
    public bool IsSpaceChar { get; set; }
  }
  internal class ParagraphBuilder : IDisposable
  {
    const int TARGET_LENGTH = 30;
    NoxStackArray noxStackArray;
    public ParagraphBuilder(NoxManagedArray flatArray)
    {
      noxStackArray = new NoxStackArray(flatArray);
      flatArray.Dispose();
    }
    public void Dispose()
    {
      noxStackArray.Dispose();
    }
    public NoxParagraph GetParagraphArray()
    {
      Stack<StackObject> char_stack = new Stack<StackObject>(noxStackArray.Value);
      Stack<StackObject> letter_stack = new Stack<StackObject>();

      Stack<StackObject[]> word_stack = new Stack<StackObject[]>();
      Stack<StackObject[][]> line_stack = new Stack<StackObject[][]>();

      try
      {
        while (char_stack.Count > 0)
        {
          var letter = char_stack.Pop();

          if (letter.IsSpaceChar || letter_stack.Count == (TARGET_LENGTH - 2))
          {
            if (letter_stack.Count > 0)
            {
              if (!letter.IsSpaceChar) letter_stack.Push(letter);
              word_stack.Push(letter_stack.ToArray());
              letter_stack.Clear();
            }
          }
          else
          {
            letter_stack.Push(letter);
          }

          int xwscount = 0;

          foreach (var ar in word_stack)
          {
            xwscount = xwscount + ar.Length;
            xwscount++;
          }

          if (xwscount == TARGET_LENGTH)
          {
            line_stack.Push(word_stack.ToArray());
            word_stack.Clear();
          }

          if (xwscount > TARGET_LENGTH)
          {
            var crstk = word_stack.Pop();
            line_stack.Push(word_stack.ToArray());

            word_stack.Clear();
            word_stack.Push(crstk);

          }
        }

        if (word_stack.Count > 0 || letter_stack.Count > 0)
        {
          if (letter_stack.Count > 0)
          {
            word_stack.Push(letter_stack.ToArray());
          }

          int wscount = 0;

          foreach (var ar in word_stack)
          {
            wscount = wscount + ar.Length;
            wscount++;
          }

          if (wscount <= TARGET_LENGTH)
          {
            line_stack.Push(word_stack.ToArray());
          }
          else
          {
            var crstk = word_stack.Pop();
            line_stack.Push(word_stack.ToArray());
            word_stack.Clear();
            word_stack.Push(crstk);
            line_stack.Push(word_stack.ToArray());
          }

        }

      }
      catch { }
     
      return new NoxParagraph(line_stack);
    }

  }
  public class NoxParagraph : IEnumerable
  {
    private List<NoxLines> _NoxLine;
    public NoxParagraph(Stack<StackObject[][]> pArray)
    {
      _NoxLine = new List<NoxLines>();

      foreach (var p1 in pArray)
      {
        List<object[]> line = new List<object[]>();

        foreach (var p2 in p1)
        {
          List<object> bitmaps = new List<object>();

          foreach (var p3 in p2)
          {
            bitmaps.Add(BitfiWallet.Nox.Sclear.GetKeyDictionary(p3.BT));
          }

          //returning space char
          bitmaps.Add(BitfiWallet.Nox.Sclear.GetKeyDictionary(32));
          line.Add(bitmaps.ToArray());
        }

        _NoxLine.Add(new NoxLines(line));
      }
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator)GetEnumerator();
    }
    public NoxEnum GetEnumerator()
    {
      return new NoxEnum(_NoxLine);
    }
  }
  public class NoxLines
  {
    private List<object[]> _char;
    public NoxLines(List<object[]> charValues)
    {
      _char = charValues;
    }
    public int GetLineCount()
    {
      return _char.Count();
    }
    public object[] GetLine(int pos)
    {
      return _char[pos];
    }

  }
  public class NoxEnum : IEnumerator
  {
    public List<NoxLines> formatedLine;

    int position = -1;
    public NoxEnum(List<NoxLines> list)
    {
      formatedLine = list;
    }
    public bool MoveNext()
    {
      position++;
      return (position < formatedLine.Count);
    }
    public void Reset()
    {
      position = -1;
    }
    object IEnumerator.Current
    {
      get
      {
        return Current;
      }
    }
    public NoxLines Current
    {
      get
      {
        try
        {
          return formatedLine[position];
        }
        catch (IndexOutOfRangeException)
        {
          throw new InvalidOperationException();
        }
      }
    }
  }
}