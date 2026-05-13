'use client';

/* Employer dashboard: overview of request status and activity
* @author:
* @since:
* @version:
*/

import { FetchApi } from "@/lib/api";
import Notification from "./Notifications";


export default function NotificationTestPanel() {

  const accessNotification = async () => {
    try {

              // fixed dummy data
            const token = localStorage.getItem("accessToken");
            const userId = localStorage.getItem("userId");

            // info below should get from when employer click the worker's info
            const workerId = "019de156-fc1a-7770-ad95-e895fa39cdd3";  // luca
            const workerInfoId = "fc4b3085-ba99-48e9-a981-ddf2e8100581";

            const response = await FetchApi('/api/notification/access', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json',
                  Authorization: `Bearer ${token}`,
                },
                body: JSON.stringify({
                  EmployerId: userId,
                  WorkerId: workerId,
                  WorkerInfoId: workerInfoId
                })
            });

            // const data = response.json();
        } catch (err) {
            console.log("error");
        }
  }

  const requestNotification = async () => {
    try {

              // fixed dummy data
            const token = localStorage.getItem("accessToken");
            const userId = localStorage.getItem("userId");

            // info below should get from when employer click the worker's info
            const workerId = "019de156-fc1a-7770-ad95-e895fa39cdd3";  // luca
            const workerInfoId = "fc4b3085-ba99-48e9-a981-ddf2e8100581";

            const response = await FetchApi('/api/notification/request', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json',
                  'Authorization': `Bearer ${token}`,
                },
                body: JSON.stringify({
                  EmployerId: userId,
                  WorkerId: workerId,
                  WorkerInfoId: workerInfoId
                })
            });

            // const data = response.json();
        } catch (err) {
            console.log("error");
        }
  }
      
  
  return (
    <div className="worker-info-access">

      {/* add a button to test the notification function */}
      {/* a button used to mimic the acceess request */}
      <div><button id="btnAccess" onClick={accessNotification}>Access Data</button></div>
      
    
      {/* a button used to mimic the request application */}
      <div><button id="btnRequest" onClick={requestNotification}>Request Application</button></div>

      {/* a demo to show all the notifications */}
      <div>
        <Notification />
      </div>


    </div>

    
  );
}
