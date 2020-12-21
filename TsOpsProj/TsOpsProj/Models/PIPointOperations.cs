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

        /*

        */
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


        public bool CreatePIPoint(PIPointModel piPoint)
        {
            string piPointName = piPoint.PointName;
            /*
            List<string> piPointList = this.GetPIPoints();
            bool piPointPresent = false;
            foreach (string point in piPointList)
            {
                if (point == piPointName)
                {
                    piPointPresent = true;
                }
            }
            */
            bool piPointPresent = PIPointExists(piPointName);
            if (!piPointPresent)
            {
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

        public bool AddPIValue(string piPoint, string dateString, string value)
        {
            bool piPointPresent = PIPointExists(piPoint);
            if (piPointPresent)
            {
                PIPoint myPoint = PIPoint.FindPIPoint(_pi, piPoint);
                PIPointType pointType = myPoint.PointType;

                object val = 0;
                try
                {
                    val = ParseValue(pointType.ToString().ToLower(), value);
                }
                catch (Exception ex)
                {
                    return false;
                }

                DateTime dateValue;
                AFTime afTime;
                if (DateTime.TryParse(dateString, out dateValue))
                {
                    afTime = new AFTime(dateValue);
                }
                else
                {
                    return false;
                }

                AFValue piValue = new AFValue(val, afTime);
                myPoint.UpdateValue(piValue, AFUpdateOption.InsertNoCompression);
                return true;
            }
            return false;

        }
    }
}