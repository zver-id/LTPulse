import {createApi, fetchBaseQuery} from "@reduxjs/toolkit/query/react";
import apiUrl from "./api-url.ts";

export interface ITableSchemaElement {
  title: string;
  dataIndex: string;
  key: string;
}

export const tableSchemaApi = createApi({
  reducerPath: 'tableSchema',
  baseQuery: fetchBaseQuery({baseUrl: `${apiUrl}/api/tableSchema`}),
  tagTypes: ["tableSchema"],

  endpoints: (builder) => ({
    getTableSchema: builder.query<ITableSchemaElement[], { typeOfTable: string }>({
      query: ({typeOfTable}) => `tableSchema=${typeOfTable}`,
      providesTags: ["tableSchema"]
    })
  }),
})

export const {
  useGetTableSchemaQuery,
} = tableSchemaApi