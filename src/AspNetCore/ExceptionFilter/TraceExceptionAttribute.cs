using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lib.AspNetCore.Logging
{
    public class TraceExceptionAttribute : Attribute, IFilterFactory
    {
        #region Fields & Properties
        
        public bool LogPostData { get; set; }
        public bool IsReusable { get; }
        public override object TypeId => _typeId;
        private object _typeId { get; }
        public string[] ExcludeDataKeys { get; set; }

        private bool HandleException { get; set; }

        #endregion

        #region Constructors


        public TraceExceptionAttribute(bool handleException)
        {

            HandleException = handleException;
            IsReusable = true;
            _typeId = new object();
            LogPostData = true;
            ExcludeDataKeys = Array.Empty<string>();

        }
        

        #endregion

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            
            return new TraceExceptionFilter(){TypeId = _typeId, LogPostData = LogPostData, ExcludeFormDataKeys = ExcludeDataKeys,HandleException = HandleException};
            
            
            

        }
    }
}
