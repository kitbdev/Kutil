using UnityEngine;
using System;

namespace Kutil {
    /// <summary>
    /// Puts this field and those below it into a collapsable foldout.
    /// Will automatically end at end of script, or a CollapsableEnd attribute.
    /// Can be configured to include or end at headers, spaces, and other decorators.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class CollapsableAttribute : PropertyAttribute {

        public string text { get; set; }
        public bool startCollapsed { get; set; } = false;

        public bool includeHeaders { get; set; } = false;
        public bool includeSpaces { get; set; } = true;
        public bool includeOtherDecorators { get; set; } = true;

        /// <summary>allows nesting of other collapsables. Use CollapsableEnd to stop</summary>
        public bool includeOtherCollapsables { get; set; } = false;
        
        public bool hideFoloutTriangle { get; set; } = false;
        public bool dontIndent { get; set; } = false;
        public bool useRichText { get; set; } = false;


        public CollapsableAttribute(string text = "", bool startCollapsed = false, bool includeHeaders = false, bool includeSpaces = true, bool includeOtherDecorators = true, bool hideFoloutTriangle = false, bool dontIndent = false, bool useRichText = false){
            this.text = text;
            this.startCollapsed = startCollapsed;
            this.includeHeaders = includeHeaders;
            this.includeSpaces = includeSpaces;
            this.includeOtherDecorators = includeOtherDecorators;
            this.hideFoloutTriangle = hideFoloutTriangle;
            this.dontIndent = dontIndent;
            this.useRichText = useRichText;
        }
    }
}