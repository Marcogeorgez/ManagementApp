using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LuminaryVisuals.Migrations;

/// <inheritdoc />
public partial class defaultForHours0 : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "SecondaryEditorDetails_Overtime",
            table: "Projects",
            type: "numeric",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "SecondaryEditorDetails_FinalBillableHours",
            table: "Projects",
            type: "numeric",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "SecondaryEditorDetails_BillableHours",
            table: "Projects",
            type: "numeric",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "SecondaryEditorDetails_AdjustmentHours",
            table: "Projects",
            type: "numeric",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "PrimaryEditorDetails_Overtime",
            table: "Projects",
            type: "numeric",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "PrimaryEditorDetails_FinalBillableHours",
            table: "Projects",
            type: "numeric",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "PrimaryEditorDetails_BillableHours",
            table: "Projects",
            type: "numeric",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "PrimaryEditorDetails_AdjustmentHours",
            table: "Projects",
            type: "numeric",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "CalculationDetails_Misc",
            table: "Projects",
            type: "numeric",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "CalculationDetails_FootageSize",
            table: "Projects",
            type: "numeric",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "CalculationDetails_FinalProjectBillableHours",
            table: "Projects",
            type: "numeric",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "CalculationDetails_ClientDiscount",
            table: "Projects",
            type: "numeric",
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric",
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "EditorWorkingHours",
            table: "EditorLoggingHours",
            type: "numeric(5,2)",
            precision: 5,
            scale: 2,
            nullable: false,
            defaultValue: 0m,
            oldClrType: typeof(decimal),
            oldType: "numeric(5,2)",
            oldPrecision: 5,
            oldScale: 2,
            oldNullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "SecondaryEditorDetails_Overtime",
            table: "Projects",
            type: "numeric",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric");

        migrationBuilder.AlterColumn<decimal>(
            name: "SecondaryEditorDetails_FinalBillableHours",
            table: "Projects",
            type: "numeric",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric");

        migrationBuilder.AlterColumn<decimal>(
            name: "SecondaryEditorDetails_BillableHours",
            table: "Projects",
            type: "numeric",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric");

        migrationBuilder.AlterColumn<decimal>(
            name: "SecondaryEditorDetails_AdjustmentHours",
            table: "Projects",
            type: "numeric",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric");

        migrationBuilder.AlterColumn<decimal>(
            name: "PrimaryEditorDetails_Overtime",
            table: "Projects",
            type: "numeric",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric");

        migrationBuilder.AlterColumn<decimal>(
            name: "PrimaryEditorDetails_FinalBillableHours",
            table: "Projects",
            type: "numeric",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric");

        migrationBuilder.AlterColumn<decimal>(
            name: "PrimaryEditorDetails_BillableHours",
            table: "Projects",
            type: "numeric",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric");

        migrationBuilder.AlterColumn<decimal>(
            name: "PrimaryEditorDetails_AdjustmentHours",
            table: "Projects",
            type: "numeric",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric");

        migrationBuilder.AlterColumn<decimal>(
            name: "CalculationDetails_Misc",
            table: "Projects",
            type: "numeric",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric");

        migrationBuilder.AlterColumn<decimal>(
            name: "CalculationDetails_FootageSize",
            table: "Projects",
            type: "numeric",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric");

        migrationBuilder.AlterColumn<decimal>(
            name: "CalculationDetails_FinalProjectBillableHours",
            table: "Projects",
            type: "numeric",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric");

        migrationBuilder.AlterColumn<decimal>(
            name: "CalculationDetails_ClientDiscount",
            table: "Projects",
            type: "numeric",
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric");

        migrationBuilder.AlterColumn<decimal>(
            name: "EditorWorkingHours",
            table: "EditorLoggingHours",
            type: "numeric(5,2)",
            precision: 5,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(5,2)",
            oldPrecision: 5,
            oldScale: 2);
    }
}
