using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using OSIsoft.AF.Data;
using OSIsoft.AF.Asset;
using OSIsoft.AF.PI;
using OSIsoft.AF.Time;

namespace TsOpsProj.Models
{
    public class PIPointOperations
    {

        public PIServer _pi;
        public List<string> piPointList; 

        public PIPointOperations(string piServerName)
        {
            PIServers piServers = new PIServers();
            _pi = piServers[piServerName];
            try
            {
                _pi.Connect();
                Console.WriteLine("Connected");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            piPointList = this.GetPIPoints();
        }

        /// <summary>
        /// Gets all the PI Points in the PI Server
        /// </summary>
        /// <returns>A list of PI Point names</returns>
        public List<string> GetPIPoints()
        {
            PIPointList myPointList = new PIPointList(PIPoint.FindPIPoints(_pi, "*"));
            List<string> piPointList = new List<string> { };
            foreach (PIPoint point in myPointList)
            {
                piPointList.Add(point.Name);
            }
            return piPointList;
        }

        /// <summary>
        /// Checks if a PI Point exists in the PI Server or not
        /// </summary>
        /// <param name="piPointName"></param>
        /// <returns>boolean</returns>
        public bool PIPointExists(string piPointName)
        {
            foreach (string point in piPointList)
            {
                if (point == piPointName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Create a new PI Point if it doesn't exist
        /// </summary>
        /// <param name="piPoint"></param>
        /// <returns>True if new PI Point is created</returns>
        public bool CreatePIPoint(PIPointModel piPoint)
        {
            string piPointName = piPoint.PointName;

            bool piPointPresent = PIPointExists(piPointName);
            if (!piPointPresent)
            {
                
                // Read the remaining attributes if they are supplied
                Dictionary<string, object> pointAttributes = new Dictionary<string, object>();

                if (piPoint.PointClassName != null) 
                { 
                    pointAttributes.Add(
                        PICommonPointAttributes.PointClassName, piPoint.PointClassName); 
                }
                else
                {
                    pointAttributes.Add(PICommonPointAttributes.PointClassName, "classic");
                }

                if (piPoint.PointType != null)
                {
                    pointAttributes.Add(
                        PICommonPointAttributes.PointType, piPoint.PointType);
                }
                else
                {
                    pointAttributes.Add(PICommonPointAttributes.PointType, "float32");
                }

                if (piPoint.Descriptor != null)
                {
                    pointAttributes.Add(
                        PICommonPointAttributes.Descriptor, piPoint.Descriptor);
                }
                else
                {
                    pointAttributes.Add(PICommonPointAttributes.Descriptor, "Hello World");
                }

                _pi.CreatePIPoint(piPointName, pointAttributes);
                Console.WriteLine("PIPointCreated");
                return true;
            }
            return false;
        }


        /// <summary>
        /// Extracts the snapshot value and timestamp of PI Point
        /// </summary>
        /// <param name="piPointName"></param>
        /// <returns>PIValueModel object with snapshot value, 
        /// timestamp and pointname</returns>
        public PIValueModel GetPIValue(string piPointName)
        {
            PIValueModel returnVal = new PIValueModel();
            bool piPointPresent = PIPointExists(piPointName);

            if (piPointPresent)
            { 
                PIPoint myPoint = PIPoint.FindPIPoint(_pi, piPointName);
                AFValue piValue = myPoint.CurrentValue();
                returnVal.PointName = piPointName;
                returnVal.datetime = piValue.Timestamp.ToString();
                returnVal.value = piValue.Value.ToString();
            }
            return returnVal;
        }


        /// <summary>
        /// Parses input value based on PI Point Type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns>Parsed value</returns>
        public object ParseValue(string type, string value)
        {
            switch(type)
            {
                case "int32": return Int32.Parse(value);
                case "int64": return Int64.Parse(value);
                case "float32": return Single.Parse(value);
                case "float64": return Double.Parse(value);
                case "string": return value;
                case "datetime": return DateTime.Parse(value);
                default: throw new ArgumentException("DataType is not supported");
            }
        }


        /// <summary>
        /// Adds a new value to PI Tag 
        /// </summary>
        /// <param name="piPoint"></param>
        /// <param name="dateString"></param>
        /// <param name="value"></param>
        /// <returns>True if the write\addition is successful</returns>
        public void AddPIValue(string piPoint, string dateString, string value)
        {
            // bool piPointPresent = PIPointExists(piPoint);
            try
            {
                PIPoint myPoint = PIPoint.FindPIPoint(_pi, piPoint);
                PIPointType pointType = myPoint.PointType;

                // Parse value
                object val = 0;
                try
                {
                    val = ParseValue(pointType.ToString().ToLower(), value);
                }
                catch (Exception ex)
                {
                    throw new Exception("Data format does not match PIPoint.PointType", ex);
                }
                // Parse date
                DateTime dateValue;
                AFTime afTime;
                try
                {
                    dateValue = DateTime.Parse(dateString);
                    afTime = new AFTime(dateValue);
                }
                catch (FormatException ex)
                {
                    throw; 
                }

                AFValue piValue = new AFValue(val, afTime);
                myPoint.UpdateValue(piValue, AFUpdateOption.InsertNoCompression);
            }
            catch (FormatException ex)
            {
                throw new Exception("Wrong DateTime format", ex);
            }
            catch (PIPointInvalidException ex)
            {
                throw new Exception("PI Point does not exist", ex);
                //return false;
            }

        }
    }
}