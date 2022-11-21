using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Webx.Web.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace Webx.Web.Helpers
{
    public class APIServiceHelper : IAPIServiceHelper
    {
        public async Task<IEnumerable<SelectListItem>> GetComboCounties(int districtId)
        {
            var districts = await GetDistrictsInfoAsync();
            var list = new List<SelectListItem>();
            var index = 1;

            foreach(var district in districts)
            {
                if(district.Id == districtId)
                {
                    foreach(var item in district.Municipios)
                    {
                        list.Add(new SelectListItem
                        {
                            Text = item,
                            Value = index.ToString(),
                        });

                        index++;
                    }
                }
            }

            list.Insert(0,new SelectListItem
            {
                Text = "[Select a County]",
                Value = "0",
            });          

            return list.OrderBy(l => l.Text);
        }

        public async Task<IEnumerable<SelectListItem>> GetComboDistricts()
        {

            var districts = await GetDistrictsInfoAsync();
            var list = new List<SelectListItem>();

            foreach(var item in districts)
            {
                list.Add(new SelectListItem
                {
                    Text = item.Distrito,
                    Value = item.Id.ToString(),
                });
            }

            list.Insert(0, new SelectListItem
            {
                Text = "[Select a district]",
                Value = "0"
            });          

            return list.OrderBy(l => l.Text);
        }

        public async Task<List<DistrictsViewModel>> GetDistrictsInfoAsync()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("Https://json.geoapi.pt/");
            var response = await client.GetAsync("distritos/municipios");
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var districtsInfo = JsonConvert.DeserializeObject<List<GeoPtApiViewModel>>(result);

            List<DistrictsViewModel> districts = new List<DistrictsViewModel>();

            var id = 1;

            foreach(var item in districtsInfo)
            {
                districts.Add(new DistrictsViewModel
                {
                    Distrito = item.Distrito,
                    Municipios = item.Municipios,
                    Id = id
                });

                id++;
            }

            return districts;
        }
    }
}
