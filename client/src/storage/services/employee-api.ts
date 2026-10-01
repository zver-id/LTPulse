import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import apiUrl from "./api-url.ts";

export interface IEmployee {
  id: number;
  name: string;
}

interface IRemoveEmployeeArgs {
  teamId: number;
  employeeId: number;
}

export const employeeApi = createApi({
  reducerPath: 'employee',
  baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api/Employee`}),
  tagTypes: ["employees"],

  endpoints: (builder) => ({
    getEmployeesByTeam: builder.query<IEmployee[], { teamId: number }>({
      query: ({ teamId }) => `?teamId=${teamId}`,
      providesTags: ['employees']
    }),
    getEmployees: builder.query<IEmployee[], void>({
      query: () => `all`,
      providesTags: ['employees']
    }),
    addEmployeeToTeam: builder.mutation<void, { teamId: number; employeeId: number }>({
      query: ({ teamId, employeeId }) => ({
        url: `?teamId=${teamId}`,
        method: "POST",
        body: { id: employeeId, name: "" }
      }),
      invalidatesTags: ['employees']
    }),
    removeEmployeeFromTeam: builder.mutation<void, IRemoveEmployeeArgs>({
      query: ({ teamId, employeeId }) => ({
        url: `remove/${teamId}?employeeId=${employeeId}`,
        method: "POST"
      }),
      invalidatesTags: ['employees']
    }),
  })
})

export const {
  useGetEmployeesByTeamQuery,
  useGetEmployeesQuery,
  useAddEmployeeToTeamMutation,
  useRemoveEmployeeFromTeamMutation
} = employeeApi;
