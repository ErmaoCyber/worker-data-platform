// SPDX-License-Identifier: MIT
pragma solidity ^0.8.24;

contract TransactionLog {
  // Keep this order aligned with backend BlockchainAction enum.
  // Existing values are kept for compatibility with older local test records.
  enum Action {
    PermissionRequested, // 0
    PermissionApproved,  // 1
    PermissionRejected,  // 2
    DataViewed,          // 3
    PermissionRevoked,   // 4
    RequestReviewed      // 5
  }

  /*
    Category-level / request-level access event.

    Main record:
    - employer
    - worker
    - requestId
    - category
    - action

    Details:
    - permissionIds: comma-separated permission IDs
    - itemLabels: item labels or request-level review summary

    Important:
    Do not store actual personal data values on-chain.
    Store only access event metadata.
  */
  event TransactionLogged(
    address indexed employer,
    address indexed worker,
    string requestId,
    string category,
    string permissionIds,
    string itemLabels,
    uint256 date,
    Action action
  );

  function logTransaction(
    address employer,
    address worker,
    string memory requestId,
    string memory category,
    string memory permissionIds,
    string memory itemLabels,
    uint256 date,
    Action action
  ) external {
    emit TransactionLogged(
      employer,
      worker,
      requestId,
      category,
      permissionIds,
      itemLabels,
      date,
      action
    );
  }
}
