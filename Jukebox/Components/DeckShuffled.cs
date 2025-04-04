using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// ReSharper disable PossibleLossOfFraction

namespace Jukebox.Components
{
    public class DeckShuffled<T> : IEnumerable<T>
    {
        private List<T> current;

        public DeckShuffled(IEnumerable<T> target) => current = Randomize(target).ToList();

        public void Reshuffle()
        {
            if (current.Count <= 1)
                return;
            var source1 = current.Take(Mathf.FloorToInt(current.Count / 2));
            var source2 = current.Skip(Mathf.FloorToInt(current.Count / 2));
            current = Randomize(source1).Concat(Randomize(source2)).ToList();
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

        public IEnumerator<T> GetEnumerator() => current.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => current.GetEnumerator();
    }

}