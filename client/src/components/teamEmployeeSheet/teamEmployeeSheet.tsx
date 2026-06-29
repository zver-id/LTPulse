import {useGetEmployeesByTeamQuery} from "../../storage/services/employee-api.ts";

function TeamEmployeeSheet(teamId: number){

  const {data} = useGetEmployeesByTeamQuery(teamId);

  return(
    <>
    </>
    )
}

export default TeamEmployeeSheet;