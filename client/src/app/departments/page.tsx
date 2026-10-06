import type { Metadata } from "next";
import { sections } from "@/shared/config/routes";
import { SectionPlaceholder } from "@/widgets/section-placeholder";

export const metadata: Metadata = {
  title: sections.departments.label + " | Directory Service",
};

export default function DepartmentsPage() {
  return <SectionPlaceholder title={sections.departments.label} />;
}
