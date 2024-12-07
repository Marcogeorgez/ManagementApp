using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LuminaryVisuals.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CalculationParameter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ParameterType = table.Column<string>(type: "text", nullable: false),
                    DefaultValue = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculationParameter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MigratedUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GoogleProviderKey = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    MigrationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigratedUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    HourlyRate = table.Column<decimal>(type: "numeric", nullable: true),
                    WeeksToDueDateDefault = table.Column<int>(type: "integer", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalculationOption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CalculationParameterId = table.Column<int>(type: "integer", nullable: false),
                    OptionName = table.Column<string>(type: "text", nullable: false),
                    Multiplier = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculationOption", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalculationOption_CalculationParameter_CalculationParameter~",
                        column: x => x.CalculationParameterId,
                        principalTable: "CalculationParameter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(255)", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "character varying(255)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(255)", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "character varying(255)", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientEditingGuidelines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(255)", nullable: false),
                    WebsiteLink = table.Column<string>(type: "text", nullable: true),
                    VideoStructure = table.Column<string>(type: "text", nullable: true),
                    CrossFades = table.Column<string>(type: "text", nullable: true),
                    FadeToBlack = table.Column<string>(type: "text", nullable: true),
                    BlackAndWhite = table.Column<string>(type: "text", nullable: true),
                    DoubleExposure = table.Column<string>(type: "text", nullable: true),
                    MaskingTransitions = table.Column<string>(type: "text", nullable: true),
                    LensFlares = table.Column<string>(type: "text", nullable: true),
                    OldFilmLook = table.Column<string>(type: "text", nullable: true),
                    PictureInPicture = table.Column<string>(type: "text", nullable: true),
                    OtherTransitions = table.Column<string>(type: "text", nullable: true),
                    TransitionComments = table.Column<string>(type: "text", nullable: true),
                    UseSpeeches = table.Column<string>(type: "text", nullable: true),
                    SpeechComments = table.Column<string>(type: "text", nullable: true),
                    SoundDesignEmphasis = table.Column<string>(type: "text", nullable: true),
                    SoundDesignComments = table.Column<string>(type: "text", nullable: true),
                    musicLicensingSites = table.Column<string>(type: "text", nullable: true),
                    SongSamples = table.Column<string>(type: "text", nullable: true),
                    ColorReferences = table.Column<string>(type: "text", nullable: true),
                    FilmReferences = table.Column<string>(type: "text", nullable: true),
                    ClientSamples = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientEditingGuidelines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientEditingGuidelines_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<string>(type: "character varying(255)", nullable: false),
                    PrimaryEditorId = table.Column<string>(type: "character varying(255)", nullable: true),
                    SecondaryEditorId = table.Column<string>(type: "character varying(255)", nullable: true),
                    ProjectName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FootageLink = table.Column<string>(type: "text", nullable: true),
                    Deliverables = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    MusicPreference = table.Column<string>(type: "text", nullable: true),
                    ProjectSpecifications_Resolution = table.Column<string>(type: "text", nullable: true),
                    ProjectSpecifications_Size = table.Column<string>(type: "text", nullable: true),
                    ProjectSpecifications_CameraNumber = table.Column<string>(type: "text", nullable: true),
                    ProjectSpecifications_ColorProfile = table.Column<string>(type: "text", nullable: true),
                    CalculationDetails_ClientDiscount = table.Column<decimal>(type: "numeric", nullable: true),
                    CalculationDetails_HighlightsDifficulty = table.Column<decimal>(type: "numeric", nullable: true),
                    CalculationDetails_PrePartsPrecentage = table.Column<decimal>(type: "numeric", nullable: true),
                    CalculationDetails_Resolution = table.Column<decimal>(type: "numeric", nullable: true),
                    CalculationDetails_FootageQuality = table.Column<decimal>(type: "numeric", nullable: true),
                    CalculationDetails_CameraNumber = table.Column<string>(type: "text", nullable: true),
                    CalculationDetails_FootageSize = table.Column<decimal>(type: "numeric", nullable: true),
                    CalculationDetails_Misc = table.Column<decimal>(type: "numeric", nullable: true),
                    CalculationDetails_PrePartsDuration = table.Column<string>(type: "text", nullable: true),
                    CalculationDetails_DocumentaryMulticameraDuration = table.Column<string>(type: "text", nullable: true),
                    CalculationDetails_DocumentaryMulticameraDurationHours = table.Column<string>(type: "text", nullable: true),
                    CalculationDetails_HighlightsDuration = table.Column<string>(type: "text", nullable: true),
                    CalculationDetails_SocialMediaDuration = table.Column<string>(type: "text", nullable: true),
                    CalculationDetails_FinalProjectBillableHours = table.Column<decimal>(type: "numeric", nullable: true),
                    NotesForProject = table.Column<string>(type: "text", nullable: true),
                    Link = table.Column<string>(type: "text", nullable: true),
                    ShootDate = table.Column<DateTime>(type: "DATE", nullable: false),
                    DueDate = table.Column<DateTime>(type: "DATE", nullable: false),
                    ProgressBar = table.Column<int>(type: "integer", nullable: false),
                    InternalOrder = table.Column<int>(type: "integer", nullable: true),
                    ExternalOrder = table.Column<int>(type: "integer", nullable: true),
                    WorkingMonth = table.Column<DateTime>(type: "DATE", nullable: true),
                    ClientBillableHours = table.Column<decimal>(type: "numeric", nullable: true),
                    ClientBillableAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    IsPaymentVisible = table.Column<bool>(type: "boolean", nullable: false),
                    PrimaryEditorDetails_BillableHours = table.Column<decimal>(type: "numeric", nullable: true),
                    PrimaryEditorDetails_Overtime = table.Column<decimal>(type: "numeric", nullable: true),
                    PrimaryEditorDetails_PaymentAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    PrimaryEditorDetails_AdjustmentHours = table.Column<decimal>(type: "numeric", nullable: true),
                    PrimaryEditorDetails_FinalBillableHours = table.Column<decimal>(type: "numeric", nullable: true),
                    SecondaryEditorDetails_BillableHours = table.Column<decimal>(type: "numeric", nullable: true),
                    SecondaryEditorDetails_Overtime = table.Column<decimal>(type: "numeric", nullable: true),
                    SecondaryEditorDetails_PaymentAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    SecondaryEditorDetails_AdjustmentHours = table.Column<decimal>(type: "numeric", nullable: true),
                    SecondaryEditorDetails_FinalBillableHours = table.Column<decimal>(type: "numeric", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AdminStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectId);
                    table.ForeignKey(
                        name: "FK_Projects_Users_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Projects_Users_PrimaryEditorId",
                        column: x => x.PrimaryEditorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Projects_Users_SecondaryEditorId",
                        column: x => x.SecondaryEditorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ConversionRateUSToLek = table.Column<decimal>(type: "numeric", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "character varying(255)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Settings_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNote",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TargetUserId = table.Column<string>(type: "character varying(255)", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "character varying(255)", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "character varying(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNote_Users_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserNote_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserNote_Users_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Archives",
                columns: table => new
                {
                    ArchiveId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Archives", x => x.ArchiveId);
                    table.ForeignKey(
                        name: "FK_Archives_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    ChatId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(255)", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsEditorMessage = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.ChatId);
                    table.ForeignKey(
                        name: "FK_Chats_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chats_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EditorLoggingHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(255)", nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    EditorWorkingHours = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorLoggingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EditorLoggingHours_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EditorLoggingHours_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Revisions",
                columns: table => new
                {
                    RevisionId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    RevisionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revisions", x => x.RevisionId);
                    table.ForeignKey(
                        name: "FK_Revisions_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "ProjectId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CalculationParameter",
                columns: new[] { "Id", "DefaultValue", "Description", "Name", "ParameterType" },
                values: new object[,]
                {
                    { 1, 1.0m, "Highlights Difficulty ", "HighlightsDifficulty", "Option" },
                    { 2, 1.0m, "Pre-Parts Percentage", "PrePartsPercentage", "Option" },
                    { 3, 1.0m, "Resolution of video", "Resolution", "Option" },
                    { 4, 1.0m, "Quality of Footage", "FootageQuality", "Option" },
                    { 5, 0.3m, "Multiplier for camera when there exist more than 2", "CameraMulti", "Decimal" },
                    { 6, 0.4m, "Multiplier for raw footage size when bigger than 300gb", "SizeMulti", "Decimal" }
                });

            migrationBuilder.InsertData(
                table: "CalculationOption",
                columns: new[] { "Id", "CalculationParameterId", "Multiplier", "OptionName" },
                values: new object[,]
                {
                    { 1, 1, 0.9m, "Straight Forward Linear, little mixing" },
                    { 2, 1, 1m, "Hybrid Mostly Linear" },
                    { 3, 1, 1.2m, "Movie with heavy SFX + VFX" },
                    { 4, 2, 1m, "0%" },
                    { 5, 2, 0.95m, "30%" },
                    { 6, 2, 0.85m, "60%" },
                    { 7, 2, 0.7m, "100%" },
                    { 8, 3, 1m, "1080p" },
                    { 9, 3, 1.05m, "Mixed" },
                    { 10, 3, 1.1m, "4k" },
                    { 11, 4, 1.15m, "Needs work" },
                    { 12, 4, 1.05m, "Mostly good" },
                    { 13, 4, 1m, "Great" },
                    { 14, 4, 0.9m, "Excellent" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Archives_ProjectId",
                table: "Archives",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CalculationOption_CalculationParameterId",
                table: "CalculationOption",
                column: "CalculationParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_Chats_ProjectId",
                table: "Chats",
                column: "ProjectId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_UserId",
                table: "Chats",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientEditingGuidelines_UserId",
                table: "ClientEditingGuidelines",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EditorLoggingHours_ProjectId",
                table: "EditorLoggingHours",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EditorLoggingHours_UserId_ProjectId_Date",
                table: "EditorLoggingHours",
                columns: new[] { "UserId", "ProjectId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MigratedUsers_GoogleProviderKey",
                table: "MigratedUsers",
                column: "GoogleProviderKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ClientId",
                table: "Projects",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_PrimaryEditorId",
                table: "Projects",
                column: "PrimaryEditorId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SecondaryEditorId",
                table: "Projects",
                column: "SecondaryEditorId");

            migrationBuilder.CreateIndex(
                name: "IX_Revisions_ProjectId",
                table: "Revisions",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_UpdatedByUserId",
                table: "Settings",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNote_ApplicationUserId",
                table: "UserNote",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNote_CreatedByUserId",
                table: "UserNote",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNote_TargetUserId",
                table: "UserNote",
                column: "TargetUserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Id",
                table: "Users",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Archives");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CalculationOption");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.DropTable(
                name: "ClientEditingGuidelines");

            migrationBuilder.DropTable(
                name: "EditorLoggingHours");

            migrationBuilder.DropTable(
                name: "MigratedUsers");

            migrationBuilder.DropTable(
                name: "Revisions");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "UserNote");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "CalculationParameter");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
