using System;
using System.Collections.Generic;

namespace gltf_shaders
{
    public class Shaders
    {
        public List<string> EmissiveColors { get; set; } 
        public PbrSpecularGlossiness PbrSpecularGlossiness { get; set; }
    }

    public class PbrSpecularGlossiness
    {
        public List<string> DiffuseColors { get; set; }
        public List<String> SpecularGlossiness { get; set; }
    }

    public class PbrMetallicRoughness
    {
        public List<string> MetallicRoughness { get; set; }
        public List<string> BaseColors { get; set; }
    }
}

