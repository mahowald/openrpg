using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace Utility
{

    /// <summary>
    /// A class to define certain properties that 
    /// most serializable elements should contain,
    /// e.g., a name. 
    /// </summary>
    public abstract class SerializableElement {

        private string name = null;

        [YamlMember(Alias = "name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


	    public SerializableElement()
        {
        }
    }
}
