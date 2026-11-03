import type { Metadata } from "next";
import { sections } from "@/shared/config/routes";
import { DepartmentsList } from "@/widgets/departments-list";

export const metadata: Metadata = {
  title: sections.departments.label + " | Directory Service",
};

export default function DepartmentsPage() {
  return <DepartmentsList />;
}
