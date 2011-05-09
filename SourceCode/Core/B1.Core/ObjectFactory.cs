using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace B1.Core
{
    ///<summary>
    ///   ObjectFactory allows to create any generic object, allowing to setup a dynamic dependency.
    ///</summary>
    public static class ObjectFactory
    {

        ///<summary>
        /// Instantiate the given class by loading the given assembly filename.
        ///</summary>
        /// <param name="assemblyFile">assembly filename (if not fully qualified; 
        /// current directory will be assumed)</param>
        /// <param name="className">class name</param>
        /// <param name="constructArgs">Optional arguments to pass in for construction.</param>
        public static T Create<T>(string assemblyFile, string className, params object[] constructArgs)
        {
            if (!assemblyFile.Trim().ToLower().EndsWith(".dll"))
                assemblyFile += ".dll";
            Assembly asm = Assembly.LoadFrom(assemblyFile);
            if (asm == null)
                throw new ArgumentException(
                    string.Format("Unable to load assembly: {0}{1}Please verify name and path."
                        , className, assemblyFile));

            object instance = (asm.CreateInstance(className, true, BindingFlags.CreateInstance, null
                    , constructArgs, null, null));
            if (instance != null)
                return (T)instance;
            else throw new ArgumentException(
                    string.Format("Unable to create instance: {0} in assembly: {1}{2}"
                        , className, assemblyFile, Environment.NewLine));
        }

        ///<summary>
        ///   Search the loaded assemblies and the current domain bin folder to find a public
        ///   class with default constructor which can be safely type casted back to the given
        ///   type.
        ///</summary>
        public static T CreateWithSearch<T>(string namespaceDomain, string filePattern)
        {
            // Check the loaded assembly first and then the Exe folder
            T instance = CreateFromLoadedAssembly<T>(namespaceDomain);
            return instance != null ? instance : CreateFromExeFolder<T>(filePattern);
        }

        ///<summary>
        ///   Search for the class in the loaded assemblies which can be instantiated and casted to T
        ///</summary>
        public static T CreateFromLoadedAssembly<T>(string namespaceDomain)
        {
            Assembly matchedAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(assembly => (assembly.FullName.StartsWith(namespaceDomain))
                                                && SearchType<T>(assembly) != null);

            return matchedAssembly == null ? default(T) : (T)Activator.CreateInstance(SearchType<T>(matchedAssembly));
        }

        ///<summary>
        ///   Search for the class in the loaded assemblies which can be instantiated and casted to T
        ///</summary>
        public static T CreateFromExeFolder<T>(string filePattern)
        {
            string executablePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            IEnumerable<string> assemblyFiles = Directory.GetFiles(executablePath, filePattern, SearchOption.AllDirectories);
            string matchedAssemblyFile = assemblyFiles.FirstOrDefault(
                    assemblyFile => SearchType<T>(Assembly.LoadFrom(assemblyFile)) != null);
            return matchedAssemblyFile == null ? default(T) :
                    (T)Activator.CreateInstance(SearchType<T>(Assembly.LoadFrom(matchedAssemblyFile)));
        }

        ///<summary>
        ///   Search the given assembly for a type which is public non-abstract class with no parameter constructor
        ///   and can be safely type casted to the templated type.
        ///</summary>
        public static Type SearchType<T>(Assembly assembly)
        {
            Type desiredType = typeof(T);
            return assembly.GetTypes().FirstOrDefault(type =>
                    type.IsClass
                    && desiredType.IsAssignableFrom(type)
                    && type.IsPublic
                    && !type.IsAbstract
                    && type.GetConstructor(Type.EmptyTypes) != null);
        }
    }
}
