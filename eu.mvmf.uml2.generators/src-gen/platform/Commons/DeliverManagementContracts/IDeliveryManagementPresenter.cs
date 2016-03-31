    
//****************************************************************************
//  NAME: IDeliveryManagementPresenter.cs
//****************************************************************************
//
//  Description: Contracts for the Delivery Dialog.
//
//****************************************************************************
//     Copyright (C) Siemens AG 2008. Confidential. All rights reserved.
//****************************************************************************
using System.Collections.Generic;
using System.ServiceModel;
using Siemens.WarehouseManagement.ContractInfrastructure;
using Siemens.WarehouseManagement.GenericPresenter;

namespace Siemens.WarehouseManagement.DeliveryManagementContracts
{
  ///<summary>
  /// Interface for DeliveryManagementPresenter
  ///</summary>
  [ServiceContract]
  public interface IDeliveryManagementPresenter
  {
    /// <summary>
    /// Inserts the delivery defines by the selected entity.
    /// </summary>
    /// <param name="entity">The selected entity.</param>
    /// <returns></returns>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void InsertDelivery(RemoteEntity entity);

    /// <summary>
    /// Updates the delivery defined by the selected entity.
    /// </summary>
    /// <param name="entity">The selected entity.</param>
    /// <returns></returns>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void UpdateDelivery(RemoteEntity entity);

    /// <summary>
    /// Deletes the delivery defined by the selected entity.
    /// </summary>
    /// <param name="entity">The selected entity.</param>
    /// <returns></returns>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void DeleteDelivery(RemoteEntity entity);

    /// <summary>
    /// Enables the delivery.
    /// </summary>
    /// <param name="entity">The entity.</param>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void EnableDelivery(RemoteEntity entity);

    /// <summary>
    /// Cancels the delivery.
    /// </summary>
    /// <param name="entity">The entity.</param>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void CancelDelivery(RemoteEntity entity);

    /// <summary>
    /// Deletes the selected delivery pos.
    /// </summary>
    /// <param name="entity">The selected delivery pos.</param>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void DeleteDeliveryPos(RemoteEntity entity);

    /// <summary>
    /// Updates the selected delivery pos.
    /// </summary>
    /// <param name="entity">The selected delivery pos.</param>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void UpdateDeliveryPos(RemoteEntity entity);

    /// <summary>
    /// Insert the selected delivery pos.
    /// </summary>
    /// <param name="entity">The selected delivery pos.</param>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void InsertDeliveryPos(RemoteEntity entity);

    /// <summary>
    /// Cancels the delivery position.
    /// </summary>
    /// <param name="entity">The entity.</param>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void CancelDeliveryPosition(RemoteEntity entity);

    /// <summary>
    /// Checks if there is enough material available for the selected position.
    /// </summary>
    /// <param name="entity">The selected delivery position.</param>
    /// <returns></returns>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    List<string> CheckMaterialAvailability(RemoteEntity entity);

    ///<summary>
    /// Returns all deliveries including their positions and picks.
    ///</summary>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    RemoteEntityContainer GetAllDeliveries(string deliveryTyp,
                                string deliveryStatus,
                                string vendor,
                                string startDate,
                                string deliveryPosStatus,
                                string material,
                                string targetLocation);

    /// <summary>
    /// Returns a RemoteEntityContainer with all delivery status possibilities
    /// </summary>
    /// <returns></returns>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    RemoteEntityContainer GetDeliveryStatusFilterData();

    /// <summary>
    /// Returns a RemoteEntityContainer with all deliveryPos status possibilities
    /// </summary>
    /// <returns></returns>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    RemoteEntityContainer GetDeliveryPosStatusFilterData();

    ///<summary>
    /// <remarks>IMPL:SVC_DELV_SUM_DETERMINE:IMPL</remarks>
    ///</summary>
    ///<returns></returns>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    RemoteEntityContainer GetSummarizedDeliveryQuery(int deliveryStatusValue, decimal materialId);

    /// <summary>
    /// Makes the delivery (represented by the entity) updateable if possible.
    /// This may result in a status change for the delivery and its positions.
    /// </summary>
    /// <param name="entity">An entity that represents the delivery.</param>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void MakeDeliveryUpdateable(RemoteEntity entity);

    /// <summary>
    /// Makes the delivery position (represented by the entity) updateable if possible.
    /// This may result in a status change for the delivery position and its delivery (incl. all positions).
    /// </summary>
    /// <param name="entity">An entity that represents the delivery position.</param>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void MakeDeliveryPosUpdateable(RemoteEntity entity);

    /// <summary>
    /// Makes the delivery deleteable if possible.
    /// </summary>
    /// <param name="entity">The delivery as remote entity.</param>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void MakeDeliveryDeleteable(RemoteEntity entity);

    /// <summary>
    /// Makes the delivery pos deleteable if possible.
    /// </summary>
    /// <param name="entity">The delivery positionas remote entity.</param>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void MakeDeliveryPosDeleteable(RemoteEntity entity);

    /// <summary>
    /// sets the status of the Delivery to Free
    /// </summary>
    /// <param name="entity"></param>
    [OperationContract]
    [FaultContract(typeof(PresenterFault))]
    void SetDeliveryStatusFree(RemoteEntity entity);
  }
}

//****************************************************************************
//     Copyright (C) Siemens AG 2008. Confidential. All rights reserved.
//****************************************************************************
