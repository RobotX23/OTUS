namespace InteractiveСonsole
{
    /// <summary>
    /// Пользователь системы (анемичная модель)
    /// </summary>
    public class ToDoUser
    {
        public Guid UserId { get; set; }
        public string? TelegramUserName { get; set; }
        public DateTime RegistereAt { get; set; }
        public long TelegramUserId { get; set; }
    }
}