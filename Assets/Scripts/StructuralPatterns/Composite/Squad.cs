using System.Collections.Generic;

namespace StructuralPatterns.Composite
{
    public class Squad : Unit
    {
        private readonly List<Unit> _units = new();

        public void AddUnit(Unit unit)
        {
            _units.Add(unit);
        }

        public void RemoveUnit(Unit unit)
        {
            _units.Remove(unit);
        }

        public override void Attack()
        {
            foreach (var unit in _units) unit.Attack();
        }
    }
}