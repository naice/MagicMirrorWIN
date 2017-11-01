namespace AWSMagicMirror
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class MagicMirrorDB : DbContext
    {
        // Der Kontext wurde für die Verwendung einer MagicMirrorDB-Verbindungszeichenfolge aus der 
        // Konfigurationsdatei ('App.config' oder 'Web.config') der Anwendung konfiguriert. Diese Verbindungszeichenfolge hat standardmäßig die 
        // Datenbank 'AWSMagicMirror.MagicMirrorDB' auf der LocalDb-Instanz als Ziel. 
        // 
        // Wenn Sie eine andere Datenbank und/oder einen anderen Anbieter als Ziel verwenden möchten, ändern Sie die MagicMirrorDB-Zeichenfolge 
        // in der Anwendungskonfigurationsdatei.
        public MagicMirrorDB()
            : base("name=MagicMirrorDB")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Models.Mirror>()
                .HasMany(mirror => mirror.Devices)
                .WithRequired(device => device.Mirror)
                .WillCascadeOnDelete();
        }


        // Fügen Sie ein 'DbSet' für jeden Entitätstyp hinzu, den Sie in das Modell einschließen möchten. Weitere Informationen 
        // zum Konfigurieren und Verwenden eines Code First-Modells finden Sie unter 'http://go.microsoft.com/fwlink/?LinkId=390109'.

        public virtual DbSet<Models.Mirror> Mirrors { get; set; }
        public virtual DbSet<Models.AlexaEchoDevice> Devices { get; set; }
    }
}