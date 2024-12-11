using Microsoft.AspNetCore.Mvc;
using MVC_Project.Areas.LOC_City.Models;
using MVC_Project.Areas.LOC_State.Models;
using System.Data;
using System.Data.SqlClient;
using static MVC_Project.Areas.LOC_Country.Models.LOC_CountryModel;
using System.IO;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace MVC_Project.Areas.LOC_City.Controllers
{
	[Area("LOC_City")]
	[Route("LOC_City/{controller}/{action}/{id?}")]
	public class LOC_CityController : Controller
	{
		private IConfiguration Configuration;

        #region Configuration
        public LOC_CityController(IConfiguration configuration)
		{
			Configuration = configuration;
		}
		#endregion

		#region SelectAll
		public IActionResult CityList()
		{
			DataTable dt = new DataTable();
			string connectionString = this.Configuration.GetConnectionString("myConnectionStrings");
			SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			SqlCommand command = connection.CreateCommand();
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = "PR_City_SelectAll";
			SqlDataReader reader = command.ExecuteReader();

			if (reader.HasRows)
			{
				dt.Load(reader);
			}

			List<LOC_CityModel> modelList = ConvertDataTableToList(dt);

			// Implement logic to select all visitors
			foreach (var City in modelList)
			{
				City.IsSelected = false;
			}

			return View(modelList);

		}
        #endregion

        #region ConvertDataTableToList
        private List<LOC_CityModel> ConvertDataTableToList(DataTable dt)
		{
			List<LOC_CityModel> modelList = new List<LOC_CityModel>();

			foreach (DataRow row in dt.Rows)
			{
				LOC_CityModel model = new LOC_CityModel
				{
					CityID = row["CityID"] != DBNull.Value ? (int?)row["CityID"] : null,
					CityName = row["CityName"].ToString(),
					Citycode = row["Citycode"].ToString(),
					StateID = row["StateID"] != DBNull.Value ? (int?)row["StateID"] : null,
					StateName = row["StateName"].ToString(),
					CountryName = row["CountryName"].ToString(),
					CountryID = Convert.ToInt32(row["CountryID"]),
                    CreationDate = Convert.ToDateTime(row["CreationDate"]),
					Modified = Convert.ToDateTime(row["Modified"]),
					IsSelected = true // Default to true initially
				};

				modelList.Add(model);
			}
			return modelList;
		}
        #endregion

		#region MultipleDelete
        [HttpPost]
		public IActionResult MultipleDelete(List<LOC_CityModel> model)
		{
			// Get selected IDs as a comma-separated string
			var selectedIds = model.Where(m => m.IsSelected).Select(m => m.CityID.ToString()).ToList();
			string commaSeparatedIds = string.Join(",", selectedIds);

			if (!string.IsNullOrEmpty(commaSeparatedIds))
			{
				// Call the stored procedure for multiple deletion
				if (PR_LOC_City_MultipleDelete(commaSeparatedIds))
				{
					TempData["LOC_City_Delete_AlertMessage"] = "Records Deleted Successfully";
				}
				else
				{
					TempData["LOC_City_Delete_AlertMessage"] = "Error deleting records";
				}
			}
			else
			{
				TempData["LOC_City_Delete_AlertMessage"] = "No records selected for deletion";
			}

			return RedirectToAction("CityList");
		}
        #endregion

        #region PR_LOC_City_MultipleDelete
        // Modified method to return a boolean indicating success or failure
        private bool PR_LOC_City_MultipleDelete(string cityIds)
		{
			using (SqlConnection connection = new SqlConnection(this.Configuration.GetConnectionString("myConnectionStrings")))
			{
				try
				{
					connection.Open();

					using (SqlCommand command = new SqlCommand("PR_LOC_City_MultipleDelete", connection))
					{
						command.CommandType = CommandType.StoredProcedure;

						// Add the parameter for the stored procedure
						command.Parameters.Add(new SqlParameter("@CityIDs", cityIds));

						// Execute the stored procedure
						int rowsAffected = command.ExecuteNonQuery();

						// Return true if rows were affected, indicating successful deletion
						return rowsAffected > 0;
					}
				}
				catch (Exception ex)
				{
					// Handle exception or log it
					return false;
				}
			}
		}
        #endregion

        #region Delete
        public IActionResult Delete(int CityID)
		{
			string connectionString = this.Configuration.GetConnectionString("myConnectionStrings");
			SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			SqlCommand command = connection.CreateCommand();
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = "PR_City_DeleteBYPK";
			command.Parameters.AddWithValue("@CityID", CityID);
			command.ExecuteNonQuery();
			connection.Close();
			return RedirectToAction("CityList");
		}
		#endregion

		#region City_AddEdit
		public IActionResult City_AddEdit(int? CityID)
		{
			FillCountryDDL();
            List<LOC_StateDropDownModel> loc_State = new List<LOC_StateDropDownModel>();
			ViewBag.StateList = loc_State;

            if (CityID != null)
			{
				string connectionString = this.Configuration.GetConnectionString("myConnectionStrings");
				SqlConnection connection = new SqlConnection(connectionString);
				connection.Open();
				SqlCommand objCmd = connection.CreateCommand();
				objCmd.CommandType = CommandType.StoredProcedure;
				objCmd.CommandText = "PR_City_SelectByPK";
				objCmd.Parameters.Add("@CityID", SqlDbType.Int).Value = CityID;
				SqlDataReader reader = objCmd.ExecuteReader();
				DataTable table = new DataTable();
				table.Load(reader);

				LOC_CityModel lOC_CityModel = new LOC_CityModel();
				foreach (DataRow dr in table.Rows)
				{
					lOC_CityModel.CityName = @dr["CityName"].ToString();
					lOC_CityModel.Citycode = @dr["CityCode"].ToString();
					lOC_CityModel.CountryID = Convert.ToInt32(@dr["CountryID"]);
					lOC_CityModel.StateID = Convert.ToInt32(@dr["StateID"]);
					FillStateDDL(Convert.ToInt32(@dr["CountryID"]));

				}
				return View(lOC_CityModel);
			}

			return View();
		}
		#endregion

		#region Save
		[HttpPost]
		public IActionResult Save(LOC_CityModel model)
		{
			string connectionString = this.Configuration.GetConnectionString("myConnectionStrings");
			SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			SqlCommand objCmd = connection.CreateCommand();
			objCmd.CommandType = CommandType.StoredProcedure;
			if (model.CityID == null)
			{
				objCmd.CommandText = "PR_City_Insert";
			}
			else
			{
				objCmd.CommandText = "PR_City_UpdateByPK";
				objCmd.Parameters.Add("@CityID", SqlDbType.Int).Value = model.CityID;
			}
			objCmd.Parameters.Add("@CityName", SqlDbType.VarChar).Value = model.CityName;
			objCmd.Parameters.Add("@CityCode", SqlDbType.VarChar).Value = model.Citycode;
			objCmd.Parameters.Add("@CountryID", SqlDbType.Int).Value = model.CountryID;
			objCmd.Parameters.Add("@StateID", SqlDbType.Int).Value = model.StateID;

			objCmd.ExecuteNonQuery();
			connection.Close();
			return RedirectToAction("CityList");
		}
		#endregion

		#region FillCountryDDL
		public void FillCountryDDL()
		{
			string connectionString = this.Configuration.GetConnectionString("myConnectionStrings");
			List<LOC_CountryDropDownModel> loc_Country = new List<LOC_CountryDropDownModel>();
			SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			SqlCommand objCmd = connection.CreateCommand();
			objCmd.CommandType = CommandType.StoredProcedure;
			objCmd.CommandText = "PR_Country_Dropdown";
			SqlDataReader objSDR = objCmd.ExecuteReader();
			if (objSDR.HasRows)
			{
				while (objSDR.Read())
				{
					LOC_CountryDropDownModel country = new LOC_CountryDropDownModel()
					{
						CountryID = Convert.ToInt32(objSDR["CountryID"]),
						CountryName = objSDR["CountryName"].ToString()
					};
					loc_Country.Add(country);
				}
				objSDR.Close();
			}
			connection.Close();
			ViewBag.CountryList = loc_Country;

		}
		#endregion

		#region FillStateDDL
		public void FillStateDDL(int CountryID)
		{
			string connectionString = this.Configuration.GetConnectionString("myConnectionStrings");
			List<LOC_StateDropDownModel> loc_State = new List<LOC_StateDropDownModel>();
			SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			SqlCommand objCmd = connection.CreateCommand();
			objCmd.CommandType = CommandType.StoredProcedure;
			objCmd.CommandText = "PR_State_Dropdown";
			objCmd.Parameters.Add("@CountryID",SqlDbType.Int).Value = CountryID;
			SqlDataReader objSDR = objCmd.ExecuteReader();
			if (objSDR.HasRows)
			{
				while (objSDR.Read())
				{
					LOC_StateDropDownModel state = new LOC_StateDropDownModel()
					{
						StateID = Convert.ToInt32(objSDR["StateID"]),
						StateName = objSDR["StateName"].ToString()
					};
					loc_State.Add(state);
				}
				objSDR.Close();
			}
			connection.Close();
			ViewBag.StateList = loc_State;

		}
		#endregion

		#region selectStateByCountry
		[HttpPost]
		public IActionResult SelectStateByCountry(int CountryID)
		{
			string connectionString = this.Configuration.GetConnectionString("myConnectionStrings");
			List<LOC_StateDropDownModel> loc_State = new List<LOC_StateDropDownModel>();
			SqlConnection connection = new SqlConnection(connectionString);

			//open connection and create command object.
			connection.Open();
			SqlCommand objCmd = connection.CreateCommand();
			objCmd.CommandType = CommandType.StoredProcedure;
			objCmd.CommandText = "PR_StateSelectByCountry";
			objCmd.Parameters.AddWithValue("@CountryID", CountryID);

			SqlDataReader objSDR = objCmd.ExecuteReader();
			if (objSDR.HasRows)
			{
				while (objSDR.Read())
				{
					LOC_StateDropDownModel vlst = new LOC_StateDropDownModel()
					{
						StateID = Convert.ToInt32(objSDR["StateID"]),
						StateName = objSDR["StateName"].ToString()
					};
					loc_State.Add(vlst);
				}
				objSDR.Close();
			}
			connection.Close();
			var vModel = loc_State;
			return Json(vModel);
		}

		#endregion

		#region Filtter
		public IActionResult Filtter()
		{
			string CityName = HttpContext.Request.Form["CityName"];
			string CityCode = HttpContext.Request.Form["CityCode"];
            string CountryName = HttpContext.Request.Form["CountryName"];
            string StateName = HttpContext.Request.Form["StateName"];

            string connectionString = this.Configuration.GetConnectionString("myConnectionStrings");
			SqlConnection connection = new SqlConnection(connectionString);
			connection.Open();
			SqlCommand command = connection.CreateCommand();
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = "[PR_City_SelectAll]";

			command.Parameters.AddWithValue("@CityName", CityName);
			command.Parameters.AddWithValue("@CityCode", CityCode);
            command.Parameters.AddWithValue("@CountryName", CountryName);
            command.Parameters.AddWithValue("@StateName", StateName);


            SqlDataReader reader = command.ExecuteReader();
			DataTable table = new DataTable();
			table.Load(reader);
			connection.Close();
			return View("CityList", table);
		}
        #endregion

        #region clear
        public IActionResult Clear()
        {
            return RedirectToAction("Index");
        }
		#endregion

		#region CITY_LISTForXML
		public IActionResult City_XML()
		{
			List<LOC_CityModel> cities = new List<LOC_CityModel>();

			string connectionString = this.Configuration.GetConnectionString("myConnectionStrings");
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				connection.Open();
				using (SqlCommand command = connection.CreateCommand())
				{
					command.CommandType = CommandType.StoredProcedure;
					command.CommandText = "PR_City_SelectAll";

					using (SqlDataReader reader = command.ExecuteReader())
					{
						if (reader.HasRows)
						{
							while (reader.Read())
							{
								LOC_CityModel city = new LOC_CityModel
								{
									CityID = Convert.ToInt32(reader["CityID"]),
									CityName = reader["CityName"].ToString(),
									StateID = Convert.ToInt32(reader["StateID"]),
									StateName = reader["StateName"].ToString(),
									CountryName = reader["CountryName"].ToString(),
									CountryID  = Convert.ToInt32(reader["CountryID"]),
                                    CreationDate = Convert.ToDateTime(reader["CreationDate"]),
									Modified = Convert.ToDateTime(reader["Modified"]),
									// Add other properties as needed
								};

								cities.Add(city);
							}
						}
					}
				}
			}

			return View(cities);
		}
		#endregion

		#region ClosedXML
		public FileResult EXPORT_XML()
		{
            DataTable dt = new DataTable();
            string connectionString = this.Configuration.GetConnectionString("myConnectionStrings");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PR_City_SelectAll";
            SqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                dt.Load(reader);
            }


			using (XLWorkbook wb = new XLWorkbook()) {
				wb.Worksheets.Add(dt, "abc.xlsx");
				using (MemoryStream stream = new MemoryStream())
				{
					wb.SaveAs(stream);
					return File(stream.ToArray(),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","city.xlsx");
				}
			}
		}

        #endregion

        #region AJaxCall
        public IActionResult AjaxDemo() {
			return View();
		}
        #endregion

        #region jquary
        public IActionResult jquary()
        {
            return View();
        }
        #endregion
    }
}
