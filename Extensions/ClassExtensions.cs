namespace Extensions {
    public static class ClassExtensions {
        static bool IsClass<TRequired, TChecked>(TChecked instance) => instance is TRequired;
        
    }
}