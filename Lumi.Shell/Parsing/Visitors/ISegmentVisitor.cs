using Lumi.Shell.Segments;

namespace Lumi.Shell.Parsing.Visitors
{
    public interface ISegmentVisitor
    {
        void Visit( CommandSegment segment );
        void Visit( CommandInterpolationSegment segment );
        void Visit( PipeSegment segment );
        void Visit( RedirectionSegment segment );
        void Visit( SequenceSegment segment );
        void Visit( TextSegment segment );
        void Visit( VariableSegment segment );
        void Visit( StringInterpolationSegment segment );
    }

    public interface ISegmentVisitor<out T>
    {
        T Visit( CommandSegment segment );
        T Visit( CommandInterpolationSegment segment );
        T Visit( PipeSegment segment );
        T Visit( RedirectionSegment segment );
        T Visit( SequenceSegment segment );
        T Visit( TextSegment segment );
        T Visit( VariableSegment segment );
        T Visit( StringInterpolationSegment segment );
    }
}
