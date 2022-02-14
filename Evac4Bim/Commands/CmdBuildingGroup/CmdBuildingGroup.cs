﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Forms;
using System.Reflection;
using Autodesk.Revit.UI.Selection;
using CmdBuildingGroup;
/// <summary>
/// This class allows the user to define the building group of the model 
/// Then, it changes related properties wich depend on the building group 
/// such as max travel distance and width per occupant 
/// Building groups are stored in a csv file 
/// As well as MaxAccessTravelDistance and other properties 
/// </summary>
namespace Evac4Bim
{
    [TransactionAttribute(TransactionMode.Manual)]

    public class CmdBuildingGroup : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;
            Element projInfo = doc.ProjectInformation as Element;




            // Load csv file 
            const string FUNCTION_LIST_FILE_NAME = @"\building-group.csv";
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + FUNCTION_LIST_FILE_NAME;
            string[] contents = null;
            try
            {
                contents = File.ReadAllText(path).Split('\n');

            }
            catch
            {
                TaskDialog.Show("Error", "The file <room-functions.csv> could not be opened");
                return Result.Failed;
            }

            // Parse CSV file 
            // Parse csv file
            var csv = from line in contents
                      where !String.IsNullOrEmpty(line)
                      select line.Split(',').ToArray();

            // get functions (first column)
            string[] items = getColumn(0, csv.Skip(1));

            // Get current setting
            int index = 20;
            bool s = false;
            bool a = false;
            int pos = Array.IndexOf(items, projInfo.LookupParameter("OccupancyGroup").AsString());
            if (pos>-1)
            {
                index = pos;
            }
            if (projInfo.LookupParameter("hasAlarm").AsInteger() == 1)
            {
                a = true;
            }
            if (projInfo.LookupParameter("hasSprinklers").AsInteger() == 1)
            {
                s = true;
            }




            // List of items for the combo box
            Figure f = new Figure(items, index, "Select Building group","Building group", s,  a);

            DialogResult result = f.ShowDialog();

            if (result == DialogResult.OK)
            {

                var tx = new Transaction(doc);
                tx.Start("Export IFC");


                // Edit properties 
                projInfo.LookupParameter("OccupancyGroup").Set(items[f.selectedFunctionIndex].ToString());

                Parameter hasAlarm = projInfo.LookupParameter("hasAlarm");
                Parameter hasSpk = projInfo.LookupParameter("hasSprinklers");

                if (f.hasSprinklers)
                {
                    hasSpk.Set(1);
                }
                else
                {
                    hasSpk.Set(0);
                }

                if (f.hasAlarm)
                {
                    hasAlarm.Set(1);
                }
                else
                {
                    hasAlarm.Set(0);
                }

                // init project parameters 
                setWidthPerOccupant(projInfo);
                Result r = setMaxAccessTravelDistance(projInfo);
                if (r == Result.Failed)
                {
                    return Result.Failed;
                }
                r = setTable_1006_2_1_Values(projInfo);
                if (r == Result.Failed)
                {
                    return Result.Failed;
                }




                tx.Commit();

            }

            if (result == DialogResult.Cancel)
            {
                //TaskDialog.Show("Debug", "Operation aborted");
                return Result.Cancelled;
            }

            return Result.Succeeded;


        }


        public static string[] getColumn(int index, IEnumerable<string[]> csv)
        {
            string[] res = null;

            if (index >= 0)
            {
                var columnQuery =
                from line in csv
                where !String.IsNullOrEmpty(line[index].ToString())
                select Convert.ToString(line[index]);


                res = columnQuery.ToArray();
            }

            return res;
        }


        public static void setWidthPerOccupant(Element projInfo)
        {

            string occupancyGroup = projInfo.LookupParameter("OccupancyGroup").AsString();
            int hasAlarm = projInfo.LookupParameter("hasAlarm").AsInteger();
            int hasSpk = projInfo.LookupParameter("hasSprinklers").AsInteger();

            Parameter ReqExitWidthPerOccupant = projInfo.LookupParameter("ReqExitWidthPerOccupant");
            Parameter ReqStairWidthPerOccupant = projInfo.LookupParameter("ReqStairWidthPerOccupant");

            
            // required door width per occuapant 
            // 5.1 mm if no sprinkler or alarm 
            // 3.8 mm if sprinkler + alarm + building class is not H or I-2
            if (!occupancyGroup.Contains("H") && !occupancyGroup.Contains("I2") && hasSpk ==1 && hasAlarm == 1)
            {
                ReqExitWidthPerOccupant.Set("3.8");
                ReqStairWidthPerOccupant.Set("5.1");

            }
            else
            {
                ReqExitWidthPerOccupant.Set("5.1");
                ReqStairWidthPerOccupant.Set("7.6");
            }
                

        }

        public static Result setMaxAccessTravelDistance(Element projInfo)
        {
            // Load csv file 
            const string TABLE_1017_2_FILE_NAME = @"\Table-1017-2.csv";
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + TABLE_1017_2_FILE_NAME;
            string[] contents = null;
            try
            {
                contents = File.ReadAllText(path).Split('\n');

            }
            catch
            {
                TaskDialog.Show("Error", "The file <Table-1017-2.csv> could not be opened");
                return Result.Failed;
            }

            // Parse CSV file 
            // Parse csv file
            var csv = from line in contents
                      where !String.IsNullOrEmpty(line)
                      select line.Split(',').ToArray();

            // get functions (first column)
            string[] items = getColumn(0, csv.Skip(1));

            //if building has sprinklers, load 2nd column - else, load first column
            int colIdx = 1;
            int hasSpk = projInfo.LookupParameter("hasSprinklers").AsInteger();
            if (hasSpk == 1)
            {
                colIdx = 2;
            }
            string[] lengths = getColumn(colIdx, csv.Skip(1));
            // retrieve building occupancy group and corresponding length
            int i = 20; // default value
            int pos = Array.IndexOf(items, projInfo.LookupParameter("OccupancyGroup").AsString());
            if (pos > -1)
            {
                i = pos;
            }

            // set parameter 
            projInfo.LookupParameter("1017_2_MaxExitAccessTravelDistance").Set(lengths[i].ToString());

            return Result.Succeeded;
        }


        public static Result setTable_1006_2_1_Values(Element projInfo)
        {
            // Load csv file 
            const string TABLE_1006_2_1_FILE_NAME = @"\Table-1006.2.1.csv";
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + TABLE_1006_2_1_FILE_NAME;
            string[] contents = null;
            try
            {
                contents = File.ReadAllText(path).Split('\n');

            }
            catch
            {
                TaskDialog.Show("Error", "The file <Table-1006.2.1.csv> could not be opened");
                return Result.Failed;
            }

            // Parse CSV file 
            // Parse csv file
            var csv = from line in contents
                      where !String.IsNullOrEmpty(line)
                      select line.Split(',').ToArray();

            // get functions (first column)
            string[] items = getColumn(0, csv.Skip(1));

            //if building has sprinklers, load 2nd and 3rd columns - else, load 4th column
            string[] col1 = getColumn(1, csv.Skip(1)); // 1006_2_1_MaxOccupantLoadPerRoom
            string[] col2 = getColumn(2, csv.Skip(1)); // OL < 30 ==> 1006_2_1_MaxCommonEgressDistance_Min
            string[] col3 = getColumn(3, csv.Skip(1)); // OL > 30 ==> 1006_2_1_MaxCommonEgressDistance_Max
            string[] col4 = getColumn(4, csv.Skip(1));

            // retrieve building occupancy group and corresponding index
            int i = 20; // default value
            int pos = Array.IndexOf(items, projInfo.LookupParameter("OccupancyGroup").AsString());
            if (pos > -1)
            {
                i = pos;
            }

            // set parameters 
            projInfo.LookupParameter("1006_2_1_MaxOccupantLoadPerRoom").Set(col1[i].ToString());

            // depending on presence of sprinkler or no 
            int hasSpk = projInfo.LookupParameter("hasSprinklers").AsInteger();
            if (hasSpk == 1)
            {
                // duplicate the value
                projInfo.LookupParameter("1006_2_1_MaxCommonEgressDistance_Min").Set(col4[i].ToString());
                projInfo.LookupParameter("1006_2_1_MaxCommonEgressDistance_Max").Set(col4[i].ToString());
            }
            else
            {
                projInfo.LookupParameter("1006_2_1_MaxCommonEgressDistance_Min").Set(col2[i].ToString());
                projInfo.LookupParameter("1006_2_1_MaxCommonEgressDistance_Max").Set(col3[i].ToString());

            }


            return Result.Succeeded;
        }


    }




}