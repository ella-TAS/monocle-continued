using System;

namespace Monocle {
    public class VirtualMap<T> {
        public const int SegmentSize = 50;
        public readonly int Columns;
        public readonly int Rows;
        public readonly int SegmentColumns;
        public readonly int SegmentRows;
        public readonly T? EmptyValue;

        private T?[,][,] segments;

        public VirtualMap(int columns, int rows, T? emptyValue = default) {
            Columns = columns;
            Rows = rows;
            SegmentColumns = (columns / SegmentSize) + 1;
            SegmentRows = (rows / SegmentSize) + 1;
            segments = new T?[SegmentColumns, SegmentRows][,];
            EmptyValue = emptyValue;
        }

        public VirtualMap(T[,] map, T? emptyValue = default) : this(map.GetLength(0), map.GetLength(1), emptyValue) {
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    this[x, y] = map[x, y];
        }

        public bool AnyInSegmentAtTile(int x, int y) {
            int cx = x / SegmentSize;
            int cy = y / SegmentSize;

            return segments[cx, cy] != null;
        }

        public bool AnyInSegment(int segmentX, int segmentY) {
            return segments[segmentX, segmentY] != null;
        }

        public T? InSegment(int segmentX, int segmentY, int x, int y) {
            T?[,] segment = segments[segmentX, segmentY];
            return segment == null ? EmptyValue : segment[x, y];
        }

        public T?[,]? GetSegment(int segmentX, int segmentY) {
            return segments[segmentX, segmentY];
        }

        public T? SafeCheck(int x, int y) {
            if (x >= 0 && y >= 0 && x < Columns && y < Rows)
                return this[x, y];
            return EmptyValue;
        }

        public T? this[int x, int y] {
            get {
                int cx = x / SegmentSize;
                int cy = y / SegmentSize;

                T?[,] seg = segments[cx, cy];
                if (seg == null)
                    return EmptyValue;

                return seg[x - cx * SegmentSize, y - cy * SegmentSize];
            }

            set {
                int cx = x / SegmentSize;
                int cy = y / SegmentSize;

                if (segments[cx, cy] == null) {
                    segments[cx, cy] = new T[SegmentSize, SegmentSize];

                    // fill with custom Empty Value data
                    if (EmptyValue != null && !EmptyValue.Equals(default(T)))
                        for (int tx = 0; tx < SegmentSize; tx++)
                            for (int ty = 0; ty < SegmentSize; ty++)
                                segments[cx, cy][tx, ty] = EmptyValue;
                }

                segments[cx, cy][x - cx * SegmentSize, y - cy * SegmentSize] = value;
            }
        }

        public T?[,] ToArray() {
            var array = new T?[Columns, Rows];
            for (int x = 0; x < Columns; x++)
                for (int y = 0; y < Rows; y++)
                    array[x, y] = this[x, y];
            return array;
        }

        public VirtualMap<T> Clone() {
            var clone = new VirtualMap<T>(Columns, Rows, EmptyValue);
            for (int x = 0; x < Columns; x++) {
                for (int y = 0; y < Rows; y++) {
                    T?[,] segment = segments[x, y];
                    if (segment == null)
                        continue;

                    var copy = new T?[SegmentSize, SegmentSize];
                    Array.Copy(segment, copy, segment.Length);
                    clone.segments[x, y] = copy;
                }
            }
            return clone;
        }
    }
}
