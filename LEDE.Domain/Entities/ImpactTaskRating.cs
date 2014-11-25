namespace LEDE.Domain
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ImpactTaskRating")]
    public partial class ImpactTaskRating
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RatingID { get; set; }

        public int? Lscore { get; set; }

        public int? Sscore { get; set; }

        public int? Pscore { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int VersID { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FacultyID { get; set; }

        [Key]
        [Column(Order = 3, TypeName = "date")]
        public DateTime ReviewDate { get; set; }
    }
}
