﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RedMangoApi.Models.Dto
{
    public class OrderHeaderUpdateDto
    {
        public int OrderHeaderId { get; set; }
        public string PickupName { get; set; }
        public string PickupPhoneNumber { get; set; }
        public string PickupEmail { get; set; }
        public string StripePaymentId { get; set; }
        public string Status { get; set; }

    }
}
