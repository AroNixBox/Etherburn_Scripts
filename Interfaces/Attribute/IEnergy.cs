namespace Interfaces.Attribute {
    public interface IEnergy : IAttribute {
        public bool HasEnough(float requiredAmount);
    }
}