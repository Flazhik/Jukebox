using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jukebox.Components;

public class DeckShuffled<T>(IEnumerable<T> target) : IEnumerable<T>
{
    private List<T> current = Randomize(target).ToList();

    public IEnumerator<T> GetEnumerator() => current.GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator()  => current.GetEnumerator();

    public void Reshuffle()
    {
        if (current.Count <= 1)
            return;
        current = Randomize(current).ToList();
    }

    private static IEnumerable<T> Randomize(IEnumerable<T> source)
    {
        var arr = source.ToArray();
        for (var i = arr.Length - 1; i > 0; --i)
        {
            var swapIndex = Random.Range(0, i + 1);
            yield return arr[swapIndex];
            arr[swapIndex] = arr[i];
        }

        yield return arr[0];
    }
}