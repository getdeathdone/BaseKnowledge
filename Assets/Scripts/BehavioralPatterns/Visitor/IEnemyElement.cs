namespace BehavioralPatterns.Visitor
{
    public interface IEnemyElement
    {
        void Accept(IVisitor visitor);
    }
}