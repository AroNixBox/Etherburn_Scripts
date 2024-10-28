namespace Interfaces.Attribute {
    // Use for example to Modify the Health of an Entity
    public interface IAttributeModifier {
        public void ModifyAttribute(IAttribute attribute, float amount);
    }
}