using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Webx.Web.Data.Entities;
using Webx.Web.Models;

namespace Webx.Web.Data.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointment>, IAppointmentRepository
    {
        private readonly DataContext _context;

        public AppointmentRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<string> GetAllEventsCustomerCanSeeAsync()
        {
            var appointments = await _context.Appointments.Include(a => a.WorkerID).ToListAsync();
            var orders = await _context.Orders.Include(o => o.Store).Include(o => o.Appointment).Include(o => o.Status).Include(o => o.Customer).Where(o => o.Appointment != null).ToListAsync();
            List<EventModel> eventsModel = new List<EventModel>();
            List<string> colors = new List<string>() {"#fca311", "#14213d", "#023047", "#ef233c"};
            Random r = new Random();


            foreach (var order in orders)
            {
                foreach(var appointment in appointments)
                {

                    //var date = appointment.BegginingHour.ToString("yyyy-MM-dd HH:mm:ss");


                    if(order.Appointment.Id == appointment.Id)
                    {
                        var orderDetails = await _context.OrderDetails.Include(od => od.Product).Where(od => od.Order.Id == order.Id).ToListAsync();
                        var eventName = orderDetails.Where(od => od.Product.IsService == true).FirstOrDefault().Product.Name;
                        string workerFullName = "Pending conclusion.";                      

                        if(appointment.WorkerID != null)
                        {
                            workerFullName = appointment.WorkerID.FirstName + " " + appointment.WorkerID.LastName;
                        }

                        string appointmentComments = appointment.Comments;

                        if (string.IsNullOrEmpty(appointmentComments))
                        {
                            appointmentComments = "No comments";
                        }

                        eventsModel.Add(new EventModel
                        {
                             id = appointment.Id,
                             start = appointment.BegginingHour.ToString("yyyy-MM-dd HH:mm:ss"),
                             end = appointment.EndHour.ToString("yyyy-MM-dd HH:mm:ss"),
                             eventOrderId = order.Id,
                             hasAttended = appointment.HasAttended,
                             title = eventName,
                             eventDescription = "<b>Customer:</b> " + order.Customer.FirstName + " " + order.Customer.LastName 
                             + "<br><b> Technician:</b> " + workerFullName
                             + "<br><b> Customer Comment: </b></br>" + appointmentComments
                             + "<br><b> Appointment Store: </b></br>" + order.Store.Name
                             + "<br><b> Status: </b></br>" + order.Status.Name
                        });

                        break;
                    }
                }
            }

            var events = eventsModel.Select(e => new
            {
                id = e.id,
                start = e.start,
                end = e.end,
                eventOrderId = e.eventOrderId,
                hasAttended = e.hasAttended,
                title = e.title,                
                eventDescription = e.eventDescription,
                display = "background",
                backgroundColor = colors[r.Next(colors.Count)],
                textColor="Black",
                eventClickable = false,
            }).ToList();

            return System.Text.Json.JsonSerializer.Serialize(events);         
        }


    }
}
