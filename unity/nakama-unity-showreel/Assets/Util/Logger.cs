// Comment out this line to turn off all logging - for convience
#define LOG_OUTPUT

using UnityEngine;

namespace Util
{   
    public class Logger
    {
             
        /// <summary>
        ///   <para>Logs message to the Unity Console.</para>
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void Log(object message)
        {
#if LOG_OUTPUT && (DEVELOPMENT_BUILD || UNITY_EDITOR)
          Debug.Log(message);
        }
#endif
    
        /// <summary>
        ///   <para>Logs message to the Unity Console.</para>
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void Log(object message, Object context)
        {
#if LOG_OUTPUT && (DEVELOPMENT_BUILD || UNITY_EDITOR)
          Debug.Log(message, context);
#endif
        }
    
        /// <summary>
        ///   <para>Logs a formatted message to the Unity Console.</para>
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void LogFormat(string format, params object[] args)
        {
#if LOG_OUTPUT && (DEVELOPMENT_BUILD || UNITY_EDITOR)
          Debug.LogFormat(format, args);
#endif
        }
    
        /// <summary>
        ///   <para>Logs a formatted message to the Unity Console.</para>
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void LogFormat(Object context, string format, params object[] args)
        {
#if LOG_OUTPUT && (DEVELOPMENT_BUILD || UNITY_EDITOR)
          Debug.LogFormat(context, format, args);
#endif
        }
    
        /// <summary>
        ///   <para>A variant of Debug.Log that logs an error message to the console.</para>
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void LogError(object message)
        {
#if LOG_OUTPUT && (DEVELOPMENT_BUILD || UNITY_EDITOR)
          Debug.LogError(message);
#endif
        }
    
        /// <summary>
        ///   <para>A variant of Debug.Log that logs an error message to the console.</para>
        /// </summary>
        /// <param name="message">String or object to be converted to string representation for display.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void LogError(object message, Object context)
        {
#if LOG_OUTPUT && (DEVELOPMENT_BUILD || UNITY_EDITOR)
          Debug.LogError(message, context);
#endif
        }
    
        /// <summary>
        ///   <para>Logs a formatted error message to the Unity console.</para>
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void LogErrorFormat(string format, params object[] args)
        {
#if LOG_OUTPUT && (DEVELOPMENT_BUILD || UNITY_EDITOR)
          Debug.LogErrorFormat(format, args);
#endif
        }
    
        /// <summary>
        ///   <para>Logs a formatted error message to the Unity console.</para>
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">Format arguments.</param>
        /// <param name="context">Object to which the message applies.</param>
        public static void LogErrorFormat(Object context, string format, params object[] args)
        {
#if LOG_OUTPUT && (DEVELOPMENT_BUILD || UNITY_EDITOR)
          Debug.LogErrorFormat(context, format, args);
#endif
        }
    }
}