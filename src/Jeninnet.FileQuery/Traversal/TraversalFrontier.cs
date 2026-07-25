namespace Jeninnet.FileQuery.Traversal;

internal sealed class TraversalFrontier(int initialCapacity = 64) : IDisposable {
    private TraversalFrame[] _buffer = ArrayPool<TraversalFrame>.Shared.Rent(initialCapacity);
    private int _head;
    private int _tail;

    public bool IsEmpty => _head == _tail;

    /// <summary>
    /// Push a frame onto the stack (DFS).
    /// </summary>
    /// <param name="frame">The frame to push.</param>
    public void Push(TraversalFrame frame) => Add(frame);

    /// <summary>
    /// Pop a frame from the stack (DFS).
    /// </summary>
    public TraversalFrame Pop() {
        if(IsEmpty) {
            throw new InvalidOperationException("Buffer is empty.");
        }

        return _buffer[--_tail]; // LIFO
    }

    /// <summary>
    /// Enqueue a frame into the queue (BFS).
    /// </summary>
    /// <param name="frame">The frame to enqueue.</param>
    public void Enqueue(TraversalFrame frame) => Add(frame);

    /// <summary>
    /// Dequeue a frame from the queue (BFS).
    /// </summary>
    public TraversalFrame Dequeue() {
        if(IsEmpty) {
            throw new InvalidOperationException("Buffer is empty.");
        }

        return _buffer[_head++]; // FIFO
    }

    /// <summary> Adds a frame to the buffer according to strategy </summary>
    /// <param name="frame">The frame to add.</param>
    public void Add(TraversalFrame frame) {
        EnsureCapacity();

        _buffer[_tail++] = frame;
    }

    private void EnsureCapacity() {
        if(_tail < _buffer.Length) {
            return;
        }

        var newBuffer = ArrayPool<TraversalFrame>.Shared.Rent(_buffer.Length * 2);
        Array.Copy(_buffer, _head, newBuffer, 0, _tail - _head);
        _tail -= _head;
        _head = 0;
        ArrayPool<TraversalFrame>.Shared.Return(_buffer, clearArray: true);
        _buffer = newBuffer;
    }

    public void Dispose() => ArrayPool<TraversalFrame>.Shared.Return(_buffer, clearArray: true);
}
