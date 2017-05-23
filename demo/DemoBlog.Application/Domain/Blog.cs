﻿namespace DemoBlog.Domain
{
    using System.Collections.Generic;
    using Backend.Fx.BuildingBlocks;
    using JetBrains.Annotations;

    public class Blog : AggregateRoot
    {
        [UsedImplicitly]
        private Blog()
        {}
         
        public Blog(Blogger blogger, string title, string description)
        {
            Title = title;
            Description = description;
            BloggerId = blogger.Id;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public int BloggerId { get; set; }
        public ISet<Subscriber> Subscribers { get; set; }
    }
}
