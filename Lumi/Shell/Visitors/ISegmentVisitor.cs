using Lumi.Shell.Segments;

namespace Lumi.Shell.Visitors
{
    internal interface ISegmentVisitor
    {
        void Visit( CommandSegment segment );
        void Visit( InterpolationSegment segment );
        void Visit( PipeSegment segment );
        void Visit( RedirectionSegment segment );
        void Visit( SequenceSegment segment );
        void Visit( TextSegment segment );
        void Visit( VariableSegment segment );
    }

    internal interface ISegmentVisitor<out T>
    {
        T Visit( CommandSegment segment );
        T Visit( InterpolationSegment segment );
        T Visit( PipeSegment segment );
        T Visit( RedirectionSegment segment );
        T Visit( SequenceSegment segment );
        T Visit( TextSegment segment );
        T Visit( VariableSegment segment );
    }
}
