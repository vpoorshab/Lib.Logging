using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lib.AspNetCore.Logging
{
    public class TraceActionAttribute : Attribute, IFilterFactory
    {
        #region Fields & Properties

        /// <summary>
        /// if true will log all post data , default is true
        /// </summary>
        
        public bool LogPostData { get; set; }
        public bool IsReusable { get; }
        public override object TypeId => _typeId;
        private object _typeId { get; }
        public string[] ExcludeDataKeys { get; set; }

        #endregion

        #region Constructors


        public TraceActionAttribute()
        {

            IsReusable = true;
            _typeId = new object();
            LogPostData = true;
            ExcludeDataKeys = Array.Empty<string>();

        }
        

        #endregion

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            
            return new TraceActionFilter( _typeId, LogPostData,  ExcludeDataKeys);

        }
    }
}
