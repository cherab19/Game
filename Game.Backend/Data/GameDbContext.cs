using Game.Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace Game.Backend.Data;

public sealed class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();

    public DbSet<Lobby> Lobbies => Set<Lobby>();

    public DbSet<LobbyMember> LobbyMembers => Set<LobbyMember>();

    public DbSet<GameSession> Games => Set<GameSession>();

    public DbSet<GameEvent> Events => Set<GameEvent>();

    public DbSet<LeaderboardEntry> Leaderboards => Set<LeaderboardEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("players");
            entity.HasKey(player => player.Id);
            entity.HasIndex(player => player.UsernameNormalized).IsUnique();
            entity.HasIndex(player => player.EmailNormalized).IsUnique();
            entity.Property(player => player.Username).HasMaxLength(32);
            entity.Property(player => player.UsernameNormalized).HasMaxLength(32);
            entity.Property(player => player.Email).HasMaxLength(256);
            entity.Property(player => player.EmailNormalized).HasMaxLength(256);
            entity.Property(player => player.DisplayName).HasMaxLength(64);
            entity.Property(player => player.AvatarUrl).HasMaxLength(512);
        });

        modelBuilder.Entity<Lobby>(entity =>
        {
            entity.ToTable("lobbies");
            entity.HasKey(lobby => lobby.Id);
            entity.HasIndex(lobby => lobby.Code).IsUnique();
            entity.Property(lobby => lobby.Code).HasMaxLength(12);
            entity.Property(lobby => lobby.Region).HasMaxLength(32);
            entity.Property(lobby => lobby.GameMode).HasMaxLength(32);
            entity.Property(lobby => lobby.Status).HasConversion<string>().HasMaxLength(24);

            entity.HasOne(lobby => lobby.HostPlayer)
                .WithMany(player => player.HostedLobbies)
                .HasForeignKey(lobby => lobby.HostPlayerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LobbyMember>(entity =>
        {
            entity.ToTable("lobby_members");
            entity.HasKey(member => new { member.LobbyId, member.PlayerId });

            entity.HasOne(member => member.Lobby)
                .WithMany(lobby => lobby.Members)
                .HasForeignKey(member => member.LobbyId);

            entity.HasOne(member => member.Player)
                .WithMany(player => player.LobbyMemberships)
                .HasForeignKey(member => member.PlayerId);
        });

        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.ToTable("games");
            entity.HasKey(game => game.Id);
            entity.Property(game => game.Status).HasConversion<string>().HasMaxLength(24);

            entity.HasOne(game => game.Lobby)
                .WithMany(lobby => lobby.Games)
                .HasForeignKey(game => game.LobbyId);
        });

        modelBuilder.Entity<GameEvent>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(gameEvent => gameEvent.Id);
            entity.Property(gameEvent => gameEvent.EventType).HasMaxLength(64);
            entity.Property(gameEvent => gameEvent.PayloadJson).HasColumnType("jsonb");
            entity.HasIndex(gameEvent => new { gameEvent.LobbyId, gameEvent.CreatedAtUtc });
            entity.HasIndex(gameEvent => new { gameEvent.GameId, gameEvent.CreatedAtUtc });

            entity.HasOne(gameEvent => gameEvent.Player)
                .WithMany(player => player.Events)
                .HasForeignKey(gameEvent => gameEvent.PlayerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(gameEvent => gameEvent.Lobby)
                .WithMany(lobby => lobby.Events)
                .HasForeignKey(gameEvent => gameEvent.LobbyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(gameEvent => gameEvent.Game)
                .WithMany(game => game.Events)
                .HasForeignKey(gameEvent => gameEvent.GameId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<LeaderboardEntry>(entity =>
        {
            entity.ToTable("leaderboards");
            entity.HasKey(entry => entry.PlayerId);
            entity.HasIndex(entry => entry.Score);
            entity.HasIndex(entry => entry.Wins);

            entity.HasOne(entry => entry.Player)
                .WithOne(player => player.Leaderboard)
                .HasForeignKey<LeaderboardEntry>(entry => entry.PlayerId);
        });
    }
}