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

        public async Task<string> GetAllEventsAsync()
        {
            var appointments = await _context.Appointments.Include(a => a.WorkerID).ToListAsync();
            var orders = await _context.Orders.Include(o => o.Store).Include(o => o.Appointment).Include(o => o.Status).Include(o => o.Customer).Where(o => o.Appointment != null).ToListAsync();

            var events = await GetEventsStringAsync(orders, appointments);

            return events;
        }

        public async Task<string> GetAllEventsCustomerCanSeeAsync(int storeId)
        {
            var appointments = await _context.Appointments.Include(a => a.WorkerID).ToListAsync();
            var orders = await _context.Orders.Include(o => o.Store).Include(o => o.Appointment).Include(o => o.Status).Include(o => o.Customer).Where(o => o.Appointment != null && o.Store.Id == storeId).ToListAsync();
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

        public async Task<Appointment> GetAppointmentByIdAsync(int appointmentId)
        {
            return await _context.Appointments.Include(a => a.WorkerID).Where(a => a.Id == appointmentId).FirstOrDefaultAsync();
        }

        public async Task<string> GetSpecificStoreEvents(int storeId)
        {
            var appointments = await _context.Appointments.Include(a => a.WorkerID).ToListAsync();
            var orders = await _context.Orders.Include(o => o.Store).Include(o => o.Appointment).Include(o => o.Status).Include(o => o.Customer).Where(o => o.Appointment != null && o.Store.Id == storeId).ToListAsync();
            
            var events = await GetEventsStringAsync(orders, appointments);

            return events;          
        }

        private async Task<string> GetEventsStringAsync(List<Order> orders, List<Appointment> appointments)
        {
            List<EventModel> eventsModel = new List<EventModel>();

            foreach (var order in orders)
            {
                foreach (var appointment in appointments)
                {

                    if (order.Appointment.Id == appointment.Id)
                    {
                        var orderDetails = await _context.OrderDetails.Include(od => od.Product).Where(od => od.Order.Id == order.Id).ToListAsync();
                        var eventName = orderDetails.Where(od => od.Product.IsService == true).FirstOrDefault().Product.Name;
                        string workerFullName = "Pending conclusion.";
                        var color = "";

                        if (appointment.WorkerID != null)
                        {
                            workerFullName = appointment.WorkerID.FirstName + " " + appointment.WorkerID.LastName;
                        }

                        if (appointment.HasAttended == true)
                        {
                            color = "#0163D2";
                        }
                        else
                        {
                            color = "#232323";
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
                            color = color,
                            eventDescription = "<b>Customer:</b> " + order.Customer.FirstName + " " + order.Customer.LastName
                             + "<br><b> Technician:</b> " + workerFullName
                             + "<br><b> Customer Comment: </b></br>" + appointmentComments
                             + "<br><b> Appointment Store: </b></br>" + order.Store.Name
                             + "<br><b> Status: </b></br>" + order.Status.Name,
                            eventStatus = order.Status.Name,
                            Custumer = order.Customer,
                            storeName = order.Store.Name,
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
                title = e.title +" Customer: "+e.Custumer.FullName+" Store: "+e.storeName,
                eventDescription = e.eventDescription,
                backgroundColor = e.color,
                textColor = "White",
                eventStatus = e.eventStatus,
            }).ToList();

            return System.Text.Json.JsonSerializer.Serialize(events);

        }
    }
}
