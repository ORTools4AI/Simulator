// Modified for ORTools4AI.Simulator.

using ORTools4AI.Simulator.core.exception;
using ORTools4AI.Simulator.core.report;

namespace ORTools4AI.Simulator.core.engine
{
    public class SortedMapEventList : EventList
    {
        private sealed class EventNoteKeyComparer : IComparer<EventNote>
        {
            internal static readonly EventNoteKeyComparer Instance = new EventNoteKeyComparer();

            public int Compare(EventNote? x, EventNote? y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                return x.CompareTo(y);
            }
        }

        // Level 1: sorted by (time, priority); key = representative EventNote at that slot
        private readonly SortedDictionary<EventNote, LinkedList<EventNote>> _buckets =
            new(EventNoteKeyComparer.Instance);

        // Level 2: O(1) lookup — keyed by object identity, not time+priority equality
        private readonly Dictionary<EventNote, (LinkedList<EventNote> List, LinkedListNode<EventNote> Node)> _nodeIndex =
            new(ReferenceEqualityComparer.Instance);

        public override EventNote? firstNote() =>
            _buckets.Count == 0 ? null : _buckets.First().Value.First!.Value;

        public override void insert(EventNote newNote)
        {
            addNoteToEntities(newNote);
            if (!_buckets.TryGetValue(newNote, out var list))
            {
                list = new LinkedList<EventNote>();
                _buckets[newNote] = list;
            }
            _nodeIndex[newNote] = (list, list.AddLast(newNote));
        }

        private static void addNoteToEntities(EventNote newNote)
        {
            newNote.getEntity1()?.addEventNote(newNote);
            newNote.getEntity2()?.addEventNote(newNote);
            newNote.getEntity3()?.addEventNote(newNote);
            newNote.getEvent()?.addEventNote(newNote);
        }

        public override void insertAfter(EventNote afterNote, EventNote newNote)
        {
            if (!_nodeIndex.TryGetValue(afterNote, out var entry))
                throw referenceNotFoundException(newNote, "insertAfter");
            correctTimeAndPriority(newNote, afterNote);
            var newNode = entry.List.AddAfter(entry.Node, newNote);
            _nodeIndex[newNote] = (entry.List, newNode);
            addNoteToEntities(newNote);
        }

        public override void insertAsFirst(EventNote newNote)
        {
            addNoteToEntities(newNote);
            if (_buckets.Count == 0)
            {
                var list = new LinkedList<EventNote>();
                _buckets[newNote] = list;
                _nodeIndex[newNote] = (list, list.AddFirst(newNote));
                return;
            }

            var first = _buckets.First();
            int cmp = newNote.CompareTo(first.Value.First!.Value);

            if (cmp < 0)
            {
                var newList = new LinkedList<EventNote>();
                _buckets[newNote] = newList;
                _nodeIndex[newNote] = (newList, newList.AddFirst(newNote));
            }
            else
            {
                if (cmp > 0) correctTimeAndPriority(newNote, first.Value.First!.Value);
                _nodeIndex[newNote] = (first.Value, first.Value.AddFirst(newNote));
            }
        }

        public override void insertBefore(EventNote beforeNote, EventNote newNote)
        {
            if (!_nodeIndex.TryGetValue(beforeNote, out var entry))
                throw referenceNotFoundException(newNote, "insertBefore");
            correctTimeAndPriority(newNote, beforeNote);
            var newNode = entry.List.AddBefore(entry.Node, newNote);
            _nodeIndex[newNote] = (entry.List, newNode);
            addNoteToEntities(newNote);
        }

        private static void correctTimeAndPriority(EventNote newNote, EventNote reference)
        {
            newNote.setTime(reference.getTime());
            newNote.setPriority(reference.getPriority());
        }

        private static SimAbortedException referenceNotFoundException(EventNote newNote, string method)
        {
            Model? mBuffer = newNote.getEntity1()?.getModel() ?? newNote.getEvent()?.getModel();
            return new SimAbortedException(new ErrorMessage(
                mBuffer,
                "Can not insert new event-note with reference to given EventNote! Simulation aborted",
                "Internal DESMO-J class : SortedMapEventList Method : " + method,
                "The reference event-note is not contained in the event tree list.",
                "This is a fatal error. Contact DESMOJ support",
                newNote.getTime()));
        }

        public override bool isEmpty() => _buckets.Count == 0;

        public override EventNote? lastNote() =>
            _buckets.Count == 0 ? null : _buckets.Last().Value.Last!.Value;

        public override EventNote? nextNote(EventNote origin)
        {
            if (!_nodeIndex.TryGetValue(origin, out var entry)) return null;
            if (entry.Node.Next != null) return entry.Node.Next.Value;
            bool found = false;
            foreach (var kvp in _buckets)
            {
                if (found) return kvp.Value.First!.Value;
                if (ReferenceEquals(kvp.Value, entry.List)) found = true;
            }
            return null;
        }

        public override EventNote? prevNote(EventNote origin)
        {
            if (!_nodeIndex.TryGetValue(origin, out var entry)) return null;
            if (entry.Node.Previous != null) return entry.Node.Previous.Value;
            LinkedList<EventNote>? prevList = null;
            foreach (var kvp in _buckets)
            {
                if (ReferenceEquals(kvp.Value, entry.List)) break;
                prevList = kvp.Value;
            }
            return prevList?.Last!.Value;
        }

        public override void remove(EventNote note)
        {
            if (!_nodeIndex.TryGetValue(note, out var entry)) return;
            entry.List.Remove(entry.Node);
            if (entry.List.Count == 0) _buckets.Remove(note);
            _nodeIndex.Remove(note);
            removeNoteFromEntities(note);
        }

        public override EventNote? removeFirst()
        {
            if (_buckets.Count == 0) return null;
            var first = _buckets.First();
            var node = first.Value.First!;
            first.Value.RemoveFirst();
            if (first.Value.Count == 0) _buckets.Remove(first.Key);
            _nodeIndex.Remove(node.Value);
            removeNoteFromEntities(node.Value);
            return node.Value;
        }

        private static void removeNoteFromEntities(EventNote note)
        {
            note.getEntity1()?.removeEventNote(note);
            note.getEntity2()?.removeEventNote(note);
            note.getEntity3()?.removeEventNote(note);
            note.getEvent()?.removeEventNote(note);
        }

        public override bool isRandomizingConcurrentEvents() => false;

        public override List<EventNote> iterators()
        {
            var result = new List<EventNote>();
            foreach (var kvp in _buckets)
                foreach (var note in kvp.Value)
                    result.Add(note);
            return result;
        }
    }
}
