namespace EG.IdentityManagement.Microservice.Entities.Base
{
    /// <summary>
	/// Entity base for all entities except identity related entities
	/// </summary>
	/// <typeparam name="TId"></typeparam>
    public class IEntityBase<TId>
    {
        /// <summary>
        /// Id for entities.  Type int
        /// </summary>
        private TId Id { get; }
    }
}