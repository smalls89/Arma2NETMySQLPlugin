﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.AddIn;
using AddInView;
using Arma2Net.Managed;

namespace Arma2NETMySQLPlugin
{
    [AddIn("Arma2NETMySQL")] //the function name for the plugin (called from Arma side)
    public class Arma2NETMySQLPlugin : Arma2NetAddIn
    {
        Logger logger_object = null;

        //This method is called when callExtension is used from SQF:
        //"Arma2Net.Unmanaged" callExtension "Arma2NetMySQL ..."
        public override string Run(string args)
        {
            IList<object> arguments;
            if (Format.SqfAsCollection(args, out arguments) && arguments.Count >= 2 && arguments[0] != null && arguments[1] != null)
            {
                string database = arguments[0] as string;
                string procedure = arguments[1] as string;
                string parameters = arguments[2] as string;
                //strip out [] characters at the beginning and end
                if (parameters[0].ToString() == "[" && parameters[parameters.Length - 1].ToString() == "]")
                {
                    parameters = parameters.Substring(1, parameters.Length - 2);
                }
                List<string> split = new List<string>();
                if (parameters != null)
                {
                    split = parameters.Split(',').ToList<string>();
                }

                Logger.addMessage(Logger.LogType.Info, "Received - Database: " + database + " Procedure: " + procedure + " Parameters: " + parameters.ToString());

                if (MySQL.dbs.SQLProviderExists(database))
                {
                    IEnumerable<string[]> returned = MySQL.dbs.getSQLProvider(database).RunProcedure(procedure, split.ToArray());
                    return Arma2Net.Managed.Format.ObjectAsSqf(returned);
                }
                else
                {
                    Logger.addMessage(Logger.LogType.Warning, "The database: " + database + " is not loaded in through the Databases.txt file.");
                }

                //Logger.addMessage(Logger.LogType.Info, "Returning false object");
                return Arma2Net.Managed.Format.ObjectAsSqf(false);
            }
            else
            {
                throw new FunctionNotFoundException();
            }
        }

        public Arma2NETMySQLPlugin()
        {
            //constructor

            //Start up logging
            logger_object = new Logger();

            Logger.addMessage(Logger.LogType.Info, "Arma2NETMySQL Plugin Started.");

            //Use AssemblyInfo.cs version number
            //Holy cow this is confusing...
            //http://stackoverflow.com/questions/909555/how-can-i-get-the-assembly-file-version
            //http://all-things-pure.blogspot.com/2009/09/assembly-version-file-version-product.html
            //http://stackoverflow.com/questions/64602/what-are-differences-between-assemblyversion-assemblyfileversion-and-assemblyin
            Logger.addMessage(Logger.LogType.Info, "Version number: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

            Logger.addMessage(Logger.LogType.Info, "Compiled with Arma2NET Version: " + Bridge.Version);

            //Load in Databases.txt file
            //This also sets up the SQLProvider associated with the database
            Logger.addMessage(Logger.LogType.Info, "Loading databases...");
            MySQL.dbs = new Databases();
        }
    }


    [AddIn("Arma2NETMySQLCommand")] //the function name for the plugin (called from Arma side)
    public class Arma2NETMySQLPluginCommand : Arma2NetAddIn
    {
        //This method is called when callExtension is used from SQF:
        //"Arma2Net.Unmanaged" callExtension "Arma2NetMySQLCommand ..."
        public override string Run(string args)
        {
            IList<object> arguments;
            if (Format.SqfAsCollection(args, out arguments) && arguments.Count == 2 && arguments[0] != null && arguments[1] != null)
            {
                string database = arguments[0] as string;
                string mysql_command = arguments[1] as string;

                Logger.addMessage(Logger.LogType.Info, "Received - Database: " + database + " MySQL Command: " + mysql_command.ToString());

                if (MySQL.dbs.SQLProviderExists(database))
                {
                    IEnumerable<string[]> returned = MySQL.dbs.getSQLProvider(database).RunCommand(mysql_command);
                    return Arma2Net.Managed.Format.ObjectAsSqf(returned);
                }
                else
                {
                    Logger.addMessage(Logger.LogType.Warning, "The database: " + database + " is not loaded in through the Databases.txt file.");
                }

                //Logger.addMessage(Logger.LogType.Info, "Returning false object");
                return Arma2Net.Managed.Format.ObjectAsSqf(false);
            }
            else
            {
                throw new FunctionNotFoundException();
            }
        }
    }
}
