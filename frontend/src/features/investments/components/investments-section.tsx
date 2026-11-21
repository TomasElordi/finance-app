import { getInvestments } from "../data/get-investments";
import { getInvestmentHistory } from "../data/get-investment-history";
import InvestmentsView from "./investments-view";

export default async function InvestmentsSection() {
  const [portfolio, history] = await Promise.all([getInvestments(), getInvestmentHistory()]);
  return <InvestmentsView portfolio={portfolio} history={history} />;
}
