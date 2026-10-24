using FinanceApp.Backend.Domain.Entities;
using FinanceApp.Backend.Domain.Icons;

namespace FinanceApp.Backend.Application.Services;

public static class DefaultTransactionGroups
{
  public static List<TransactionGroup> CreateFor(User user) =>
  [
    new TransactionGroup("Groceries", "Payment for groceries", Icons.ICON_GROCERIES, user),
    new TransactionGroup("Entertainment", "Expense for entertainment", Icons.ICON_ENTERTAINMENT, user),
    new TransactionGroup("Car", "Car cost and fuel", Icons.ICON_CAR, user),
    new TransactionGroup("Transport", "Transport costs like ticket or pass", Icons.ICON_TRANSPORT, user),
    new TransactionGroup("Health", "Medical and health-related expenses", Icons.ICON_HEALTH, user),
    new TransactionGroup("Utilities", "Utility bills and services", Icons.ICON_UTILITIES, user),
    new TransactionGroup("Education", "Education and learning expenses", Icons.ICON_EDUCATION, user),
    new TransactionGroup("Travel", "Travel and vacation expenses", Icons.ICON_TRAVEL, user),
    new TransactionGroup("Dining", "Restaurant and dining out", Icons.ICON_DINING, user),
    new TransactionGroup("Gifts", "Gifts and donations", Icons.ICON_GIFTS, user),
    new TransactionGroup("Salary", "Income from salary or wages", Icons.ICON_SALARY, user),
    new TransactionGroup("Home", "Home maintenance and rent", Icons.ICON_HOME, user),
    new TransactionGroup("Personal Care", "Personal care and hygiene", Icons.ICON_PERSONAL_CARE, user),
    new TransactionGroup("Clothing", "Clothing and accessories", Icons.ICON_CLOTHING, user),
    new TransactionGroup("Phone & Internet", "Phone, internet, and communication", Icons.ICON_PHONE_INTERNET, user),
    new TransactionGroup("Taxes", "Tax payments and refunds", Icons.ICON_TAXES, user),
    new TransactionGroup("Miscellaneous", "Other uncategorized expenses", Icons.ICON_MISCELLANEOUS, user),
    new TransactionGroup("Sold Items", "Sold personal items", Icons.ICON_SALARY, user),
    new TransactionGroup("Interests", "Interests after savings or investments", Icons.ICON_SALARY, user)
  ];
}
