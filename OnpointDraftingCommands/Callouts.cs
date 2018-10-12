using System;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;


[assembly: CommandClass(typeof(OnpointDraftingCommands.Callouts))]
namespace OnpointDraftingCommands
{
    class Callouts
    {
        // Get the current document and database
        Document acDoc = Application.DocumentManager.MdiActiveDocument;
        Database acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
        public const string TAP_PED = "PLACE TAP PEDESTAL\r\n" +
                                      "10\"%%c x 17\"h 18\" STAKE";
        public const string DROP_BUCKET = "PLACE DROP BUCKET\r\n" +
                                          "9.5\"%%c x 17\"D, FLUSH";
        public const string SPLITTER_PED = "PLACE SPLITTER PED\r\n" +
                                           "10\"%%c x 22\"H 24\" STAKE";
        public const string LE_PED = "PLACE LE PEDESTAL\r\n" +
                                     "12\"W x 12\"L x 32\"H";
        public const string AMP_PED = "PLACE AMP PEDESTAL\r\n" +
                                      "15\"W x 34\"L x 16\"H";
        public const string NODE_PED = "PLACE NODE PEDESTAL\r\n" +
                                       "26\"W x 38\"L x 24\"H";
        public const string POWER_SUPPLY = "PLACE POWER SUPPLY\r\n" +
                                           "26\"W x 15\"L x 34\"H";
		public const string UGUARD_RISER = "PLACE U-GUARD RISER\r\n" +
											"ON EXISTING POLE";
		
        public const int recWidth = 10;
        public const int recLength = 45;

        [CommandMethod("cout")]

        public void cout()
        {
            Editor ed = acDoc.Editor;
            //Prompt options for running line
            PromptEntityOptions peo = new PromptEntityOptions("Select Running Line");
            peo.SetRejectMessage("Running line not selected");
            peo.AddAllowedClass(typeof(Polyline), false);

            PromptEntityResult perRunningLine = ed.GetEntity(peo);
            if (perRunningLine.Status != PromptStatus.OK)
                return;

            PromptStringOptions pStrOpts = new PromptStringOptions("\nEnter Line Number: ");
            pStrOpts.AllowSpaces = false;
            PromptResult pStrRes = acDoc.Editor.GetString(pStrOpts);


            while (true)
            {

                Transaction trans = acCurDb.TransactionManager.StartTransaction();

                using (trans)
                {

                    // Open the Block table for read
                    BlockTable acBlkTbl;
                    acBlkTbl = trans.GetObject(acCurDb.BlockTableId,
                                                  OpenMode.ForRead) as BlockTable;

                    // Open the Block table record Model space for write
                    BlockTableRecord acBlkTblRec;
                    acBlkTblRec = trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace],
                                                    OpenMode.ForWrite) as BlockTableRecord;

                    //prompt for the block
                    PromptEntityOptions peo2 = new PromptEntityOptions("\nSelect Block");
                    peo2.SetRejectMessage("\nnot a block");
                    peo2.AddAllowedClass(typeof(BlockReference), false);

                    PromptEntityResult perBlock = ed.GetEntity(peo2);
                    if (perBlock.Status != PromptStatus.OK)
                        return;

                    Polyline runningLine = trans.GetObject(perRunningLine.ObjectId, OpenMode.ForRead) as Polyline;

                    BlockReference blkRef = trans.GetObject(perBlock.ObjectId, OpenMode.ForRead) as BlockReference;

                    string str = String.Format("{0:0}", runningLine.GetDistAtPoint(blkRef.Position));
                    switch (str.Length)
                    {
                        case 1:
                            str = "0+0" + str;
                            break;
                        case 2:
                            str = "0+" + str;
                            break;
                        default:
                            str = str.Substring(0, str.Length - 2) + "+" + str.Substring(str.Length - 2);
                            break;
                    }

                    if (pStrRes.StringResult != "")
                        str = str + " LINE " + pStrRes.StringResult;

                    //prompt for insertion point
                    PromptPointOptions pPtOpt = new PromptPointOptions("\nEnter Insertion Point");
                    PromptPointResult pPtRes = ed.GetPoint(pPtOpt);
                    Point3d insPt = pPtRes.Value;

                    CoordinateSystem3d cs = Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem.CoordinateSystem3d;
                    Plane plane = new Plane(Point3d.Origin, cs.Zaxis);

                    //create polyline
                    Polyline rec = new Polyline();
                    rec.AddVertexAt(0, insPt.Convert2d(plane), 0, 0, 0);

                    bool Xdir = true;
                    bool Ydir = true;

                    if (insPt.X < blkRef.Position.X)
                        Xdir = false;
                    //

                    if (insPt.Y < blkRef.Position.Y)
                        Ydir = false;

                    if (Xdir)
                    {
                        if (Ydir)
                        {
                            //quadrant I
                            rec.AddVertexAt(0, new Point2d(insPt.X + recLength, insPt.Y), 0, 0, 0);
                            rec.AddVertexAt(1, new Point2d(insPt.X + recLength, insPt.Y + recWidth), 0, 0, 0);
                            rec.AddVertexAt(2, new Point2d(insPt.X, insPt.Y + recWidth), 0, 0, 0);
                        }
                        else
                        {
                            //quadrant IV
                            rec.AddVertexAt(0, new Point2d(insPt.X + recLength, insPt.Y), 0, 0, 0);
                            rec.AddVertexAt(1, new Point2d(insPt.X + recLength, insPt.Y - recWidth), 0, 0, 0);
                            rec.AddVertexAt(2, new Point2d(insPt.X, insPt.Y - recWidth), 0, 0, 0);
                        }
                    }
                    else
                    {
                        if (Ydir)
                        {
                            //quadrant II
                            rec.AddVertexAt(0, new Point2d(insPt.X - recLength, insPt.Y), 0, 0, 0);
                            rec.AddVertexAt(1, new Point2d(insPt.X - recLength, insPt.Y + recWidth), 0, 0, 0);
                            rec.AddVertexAt(2, new Point2d(insPt.X, insPt.Y + recWidth), 0, 0, 0);
                        }
                        else
                        {
                            //quadrant III
                            rec.AddVertexAt(0, new Point2d(insPt.X - recLength, insPt.Y), 0, 0, 0);
                            rec.AddVertexAt(1, new Point2d(insPt.X - recLength, insPt.Y - recWidth), 0, 0, 0);
                            rec.AddVertexAt(2, new Point2d(insPt.X, insPt.Y - recWidth), 0, 0, 0);
                        }

                    }
                    rec.Closed = true;
                    rec.SetDatabaseDefaults();

                    //create leader
                    Leader leader = new Leader();
                    leader.SetDatabaseDefaults();
                    leader.Layer = "DESIGN";



                    //create for station
                    MText txt = new MText();
                    txt.SetDatabaseDefaults();
                    txt.Contents = "STA " + str;
                    txt.Layer = "TEXT-2";
                    txt.TextHeight = 2.2;
                    txt.Layer = "TEXT-2";

                    //body
                    MText acMText = new MText();
                    acMText.SetDatabaseDefaults();
                    acMText.TextHeight = 2.2;
                    acMText.Layer = "TEXT-2";

                    if (Xdir)
                    {
                        if (Ydir)
                        {
                            txt.Location = new Point3d(insPt.X + 1.33, insPt.Y + 11.1833, 0);
                            txt.Attachment = AttachmentPoint.BottomLeft;

                            acMText.Location = new Point3d(insPt.X + 1.65, insPt.Y + 4.95, 0);
                            acMText.Attachment = AttachmentPoint.MiddleLeft;

                            leader.AppendVertex(new Point3d(blkRef.Position.X + 1.5, blkRef.Position.Y + 1.5, 0));
                        }
                        else
                        {
                            txt.Location = new Point3d(insPt.X + 1.33, insPt.Y + 1.2833, 0);
                            txt.Attachment = AttachmentPoint.BottomLeft;

                            acMText.Location = new Point3d(insPt.X + 1.65, insPt.Y - 4.95, 0);
                            acMText.Attachment = AttachmentPoint.MiddleLeft;

                            leader.AppendVertex(new Point3d(blkRef.Position.X + 1.5, blkRef.Position.Y - 1.5, 0));
                        }
                    }
                    else
                    {
                        if (Ydir)
                        {
                            txt.Location = new Point3d(insPt.X - 42.6643, insPt.Y + 11.1833, 0);
                            txt.Attachment = AttachmentPoint.BottomLeft;

                            acMText.Location = new Point3d(insPt.X - 42.35, insPt.Y + 4.95, 0);
                            acMText.Attachment = AttachmentPoint.MiddleLeft;

                            leader.AppendVertex(new Point3d(blkRef.Position.X - 1.5, blkRef.Position.Y + 1.5, 0));
                        }
                        else
                        {
                            txt.Location = new Point3d(insPt.X - 42.6643, insPt.Y + 1.2833, 0);
                            txt.Attachment = AttachmentPoint.BottomLeft;

                            acMText.Location = new Point3d(insPt.X - 42.6383, insPt.Y - 4.95, 0);
                            acMText.Attachment = AttachmentPoint.MiddleLeft;

                            leader.AppendVertex(new Point3d(blkRef.Position.X - 1.5, blkRef.Position.Y - 1.5, 0));
                        }
                    }

                    leader.AppendVertex(insPt);

                    MText addText = null;
                    //create text for body
                    string verbose;
                    switch (blkRef.Name.ToUpper())
                    {
                        case "CPR":
                            verbose = DROP_BUCKET;
                            break;

                        case "CPR_T":
                            verbose = TAP_PED;

                            addText = new MText();
                            addText.SetDatabaseDefaults();
                            addText.TextHeight = 2.2;
                            addText.Layer = "TEXT-2";
                            addText.Attachment = AttachmentPoint.TopLeft;
                            addText.Location = new Point3d(acMText.Location.X, acMText.Location.Y - 7, 0);
                            addText.Contents = "EXPRESS: BYPASS PED";
                            break;

                        case "CPS_S":
                            verbose = SPLITTER_PED;
                            break;

                        case "CPS":
                            verbose = LE_PED;
                            break;

                        case "DH":
                            verbose = AMP_PED;

                            addText = new MText();
                            addText.SetDatabaseDefaults();
                            addText.TextHeight = 2.2;
                            addText.Layer = "TEXT-2";
                            addText.Attachment = AttachmentPoint.TopLeft;
                            addText.Location = new Point3d(acMText.Location.X, acMText.Location.Y - 7, 0);
                            addText.Contents = "BASE 16\" DEEP";
                            break;

                        case "DH_N":
                            verbose = NODE_PED;

                            addText = new MText();
                            addText.SetDatabaseDefaults();
                            addText.TextHeight = 2.2;
                            addText.Layer = "TEXT-2";
                            addText.Attachment = AttachmentPoint.TopLeft;
                            addText.Location = new Point3d(acMText.Location.X, acMText.Location.Y - 7, 0);
                            addText.Contents = "BASE 24\" DEEP";
                            break;

                        case "PS":
                            verbose = POWER_SUPPLY;
                            break;

                        default:
                            verbose = "BLOCK TYPE NOT RECOGNIZED";
                            break;
                    }
                    acMText.Contents = verbose;

                    if (addText != null)
                    {
                        acBlkTblRec.AppendEntity(addText);
                        trans.AddNewlyCreatedDBObject(addText, true);
                    }

                    //append to block table 
                    acBlkTblRec.AppendEntity(leader);
                    trans.AddNewlyCreatedDBObject(leader, true);

                    acBlkTblRec.AppendEntity(acMText);
                    trans.AddNewlyCreatedDBObject(acMText, true);

                    acBlkTblRec.AppendEntity(rec);
                    trans.AddNewlyCreatedDBObject(rec, true);

                    acBlkTblRec.AppendEntity(txt);
                    trans.AddNewlyCreatedDBObject(txt, true);

                    trans.Commit();
                }
            }
        }
    }

}
