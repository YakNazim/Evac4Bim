﻿using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Media.Imaging;
using System.IO;

namespace Evac4Bim
{
    public class MainApp : IExternalApplication
    {
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            // Create ribbon
            application.CreateRibbonTab("Evac4Bim");
            string path2 = "";

            // Create ribbon panel
            RibbonPanel panel = application.CreateRibbonPanel("Evac4Bim", "Import/Export");
            // Create ribbon panel
            RibbonPanel panel2 = application.CreateRibbonPanel("Evac4Bim", "Tools");


            // Add a command to the ribbon 
            path2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CmdExportIfc.dll";
            PushButtonData cmdExportButton = new PushButtonData("cmdExportbutton", "Export to\nIFC2x3", path2, "Evac4Bim.CmdExportIfc");
            // Create img icon 
            Uri cmdExportPushButtonImgPath = new Uri(@"D:\revit_api\Evac4Bim\CmdExportIfc.png");
            BitmapImage cmdExportPushButtonImg = new BitmapImage(cmdExportPushButtonImgPath);    
            PushButton cmdExportPushButton =  panel.AddItem(cmdExportButton) as PushButton;
            cmdExportPushButton.LargeImage = cmdExportPushButtonImg;



            // Add a command to the ribbon 
            path2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CmdImportParameter.dll";
            PushButtonData CmdImportParametersButton = new PushButtonData("CmdImportParametersButton", "Pathfinder\u2122\nResults", path2, "Evac4Bim.ImportParameters");
            // Create img icon 
            Uri CmdImportParametersButtonImgPath = new Uri(@"D:\revit_api\Evac4Bim\ImportParameters.png");
            BitmapImage CmdImportParametersButtonImg = new BitmapImage(CmdImportParametersButtonImgPath);
            PushButton CmdImportParametersPushButton = panel.AddItem(CmdImportParametersButton) as PushButton;
            CmdImportParametersPushButton.LargeImage = CmdImportParametersButtonImg;


            // Add a command to the ribbon 
            path2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CmdLoadParameters.dll";
            PushButtonData CmdLoadParametersButton = new PushButtonData("CmdLoadParametersButton", "Generate\nParameters", path2, "Evac4Bim.CmdLoadParameters");
            // Create img icon 
            Uri CmdLoadParametersButtonImgPath = new Uri(@"D:\revit_api\Evac4Bim\CmdLoadParameters.png");
            BitmapImage CmdLoadParametersButtonImg = new BitmapImage(CmdLoadParametersButtonImgPath);
            PushButton CmdLoadParametersPushButton = panel2.AddItem(CmdLoadParametersButton) as PushButton;
            CmdLoadParametersPushButton.LargeImage = CmdLoadParametersButtonImg;

            // Add a command to the ribbon 
            path2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\CmdRenameItems.dll";
            PushButtonData CmdRenameItemsButton = new PushButtonData("CmdRenameItemsButton", "Rename\nElements", path2, "Evac4Bim.CmdRenameItems");
            // Create img icon 
            Uri CmdRenameItemsButtonImgPath = new Uri(@"D:\revit_api\Evac4Bim\CmdRenameItems.png");
            BitmapImage CmdRenameItemsButtonImg = new BitmapImage(CmdRenameItemsButtonImgPath);
            PushButton CmdRenameItemsPushButton = panel2.AddItem(CmdRenameItemsButton) as PushButton;
            CmdRenameItemsPushButton.LargeImage = CmdRenameItemsButtonImg;


            // Add a command to the ribbon 
            path2 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+ @"\CmdPlotCharts.dll";
            PushButtonData CmdPlotChartsButton = new PushButtonData("CmdPlotChartsButton", "Plot\nChart", path2, "Evac4Bim.CmdPlotCharts");
            // Create img icon 
            Uri CmdPlotChartsButtonImgPath = new Uri(@"D:\revit_api\Evac4Bim\CmdPlotCharts.png");
            BitmapImage CmdPlotChartsButtonImg = new BitmapImage(CmdPlotChartsButtonImgPath);
            PushButton CmdPlotChartsPushButton = panel2.AddItem(CmdPlotChartsButton) as PushButton;
            CmdPlotChartsPushButton.LargeImage = CmdPlotChartsButtonImg;



            return Result.Succeeded;




        }
    }
    
}
