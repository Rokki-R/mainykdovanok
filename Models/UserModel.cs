﻿namespace mainykdovanok.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int devicesWon { get; set; }
        public int devicesGifted { get; set; }
        public int Status { get; set; }
        public int Role { get; set; }
    }
}
