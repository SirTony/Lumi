﻿using Lumi.Shell.Visitors;
using System.Collections.Generic;

namespace Lumi.Shell.Segments
{
    internal sealed class PipeSegment : IShellSegment
    {
        public IShellSegment Left { get; }
        public IShellSegment Right { get; }
        public IShellSegment Parent { get; set; }

        public T Accept<T>( ISegmentVisitor<T> visitor )
            => visitor.Visit( this );

        public void Accept( ISegmentVisitor visitor )
            => visitor.Visit( this );

        public PipeSegment( IShellSegment parent, IShellSegment left, IShellSegment right )
        {
            this.Parent = parent;
            this.Left = left;
            this.Right = right;
        }

        public ShellResult Execute( IReadOnlyList<string> inputs = null, bool capture = false )
        {
            var left = this.Left.Execute( capture: true );
            return left.ExitCode != 0 ? left : this.Right.Execute( left.StandardOutput, true );
        }

        public override string ToString()
            => $"{this.Left} | {this.Right}";
    }
}